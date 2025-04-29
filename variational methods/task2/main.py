import numpy as np
from scipy.interpolate import RegularGridInterpolator
from skimage.filters import sobel, gaussian
import matplotlib.pyplot as plt
import skimage.io as skio
import argparse

def display_image_in_actual_size(img, dpi = 80):
    height = img.shape[0]
    width = img.shape[1]

    # What size does the figure need to be in inches to fit the image?
    figsize = width / float(dpi), height / float(dpi)

    # Create a figure of the right size with one axes that takes up the full figure
    fig = plt.figure(figsize=figsize, dpi=dpi)
    ax = fig.add_axes([0, 0, 1, 1])

    # Hide spines, ticks, etc.
    ax.axis('off')

    # Display the image.
    ax.imshow(img, cmap='gray')

#     plt.show()
    
    return fig, ax

def save_mask(fname, snake, img):
    plt.ioff()
    fig, ax = display_image_in_actual_size(img)
    ax.fill(snake[:, 0], snake[:, 1], '-b', lw=3)
    ax.axes.get_xaxis().set_visible(False)
    ax.axes.get_yaxis().set_visible(False)
    ax.set_frame_on(False)
    fig.savefig(fname, pad_inches=0, bbox_inches='tight', dpi='figure')
    plt.close(fig)
    
    mask = skio.imread(fname)
    blue = ((mask[:,:,2] == 255) & (mask[:,:,1] < 255) & (mask[:,:,0] < 255)) * 255
    blue = blue.astype(np.uint8)
    skio.imsave(fname, blue)
    plt.ion()
    
def display_snake(img, init_snake, result_snake):
    fig, ax = display_image_in_actual_size(img)
    ax.plot(init_snake[:, 0], init_snake[:, 1], '-r', lw=2)
    ax.plot(result_snake[:, 0], result_snake[:, 1], '-b', lw=2)
    ax.set_xticks([]), ax.set_yticks([])
    ax.axis([0, img.shape[1], img.shape[0], 0])
    plt.show()

def active_contours(image, snake, alpha, beta, w_line, w_edge, tau):
    max_iter = 3000
    history_size = 5
    conv_threshold = 0.1
    image = gaussian(image, sigma=3, preserve_range=False)
    img = image.astype(np.float32, copy=False)
    
    x = snake[:, 0].astype(np.float32)
    y = snake[:, 1].astype(np.float32)
    n = len(x)
    x_prev = np.empty((history_size, n), dtype=np.float32)
    y_prev = np.empty((history_size, n), dtype=np.float32)
    
    I = np.eye(n, dtype=float)
    a = (2 * I - np.roll(I, -1, axis=0) - np.roll(I, -1, axis=1))
    b = (np.roll(I, -2, axis=0) + np.roll(I, -2, axis=1) - 4 * np.roll(I, -1, axis=0)
        - 4 * np.roll(I, -1, axis=1) + 6 * I)
    A = alpha * a + beta * b
    inv = np.linalg.inv(tau * A + I).astype(np.float32, copy=False)

    edge = [sobel(img)]
    img = w_line * img + w_edge * edge[0]

    gy, gx = np.gradient(img)

    for i in range(max_iter):
        fx = RegularGridInterpolator(
            (np.arange(img.shape[0]), np.arange(img.shape[1])),
            gx,
            method='linear',
            bounds_error=False,
            fill_value=0,
        )(np.stack([y, x], axis=-1)).astype(np.float32, copy=False)
        fy = RegularGridInterpolator(
            (np.arange(img.shape[0]), np.arange(img.shape[1])),
            gy,
            method='linear',
            bounds_error=False,
            fill_value=0,
        )(np.stack([y, x], axis=-1)).astype(np.float32, copy=False)

        xn = inv @ (fx * tau + x)
        yn = inv @ (fy * tau + y)
        x += np.clip(xn - x, -1, 1)
        y += np.clip(yn - y, -1, 1)

        check_iter = i % (history_size + 1)
        if check_iter < history_size:
            x_prev[check_iter, :] = x
            y_prev[check_iter, :] = y
        else:
            dist = np.min(np.max(np.abs(x_prev - x[None, :]) + np.abs(y_prev - y[None, :]), 1))
            if dist < conv_threshold:
                break

    return np.stack([x, y], axis=1)

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument('input_image', type=str)
    parser.add_argument('initial_snake', type=str)
    parser.add_argument('output_image', type=str)
    parser.add_argument('alpha', type=float)
    parser.add_argument('beta', type=float)
    parser.add_argument('tau', type=float)
    parser.add_argument('w_line', type=float)
    parser.add_argument('w_edge', type=float)
    parser.add_argument('--kappa', type=float)
    
    args = parser.parse_args()
    input_image = skio.imread(args.input_image)
    init_snake = np.loadtxt(args.initial_snake)
    final_snake = active_contours(
        input_image, init_snake,
        args.alpha, args.beta, args.w_line, args.w_edge, args.tau
        )
    save_mask(args.output_image, final_snake, input_image)
