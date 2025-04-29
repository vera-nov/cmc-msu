import argparse
import numpy as np
import skimage.io


def convolve(image, kernel, radius):
    output = np.zeros_like(image).astype(np.float64)
    padded_image = np.pad(image, pad_width=radius, mode='edge')
    for i in range(image.shape[0]):
        for j in range(image.shape[1]):
            region = padded_image[i:i+kernel.shape[0], j:j+kernel.shape[1]]
            output[i, j] = np.sum(region * kernel)
    return output


def grad(sigma, img):
    radius = int(np.ceil(3 * sigma))
    x, y = np.meshgrid(np.arange(-radius, radius+1), np.arange(-radius, radius+1))
    gaussian = np.exp(-(x**2 + y**2) / (2 * sigma**2)) / (2 * np.pi * sigma**2)
    gaussian_dx = -x / (sigma**2) * gaussian
    gaussian_dy = -y / (sigma**2) * gaussian
    
    Sx = convolve(img, gaussian_dx, radius)
    Sy = convolve(img, gaussian_dy, radius)
    grad = np.sqrt(Sx**2 + Sy**2)
    
    gmax = np.max(grad)
    if gmax > 0:
        grad = (grad / gmax) * 255
    direction = np.arctan2(Sy, Sx) * (180 / np.pi)
    direction = (direction + 180) % 180
    
    return grad, direction

    
def nonmax(sigma, img):
    magn, direction = grad(sigma, img)
    rows, cols = magn.shape
    res = np.zeros_like(magn)
    
    for i in range(1, rows-1):
        for j in range(1, cols-1):
            angle = direction[i, j]
            q, r = 255, 255
            if (0 <= angle < 22.5) or (157.5 <= angle <= 180):
                q = magn[i, j+1]
                r = magn[i, j-1]
            elif 22.5 <= angle < 67.5:
                q = magn[i+1, j+1]
                r = magn[i-1, j-1]
            elif 67.5 <= angle < 112.5:
                q = magn[i+1, j]
                r = magn[i-1, j]
            elif 112.5 <= angle < 157.5:
                q = magn[i-1, j+1]
                r = magn[i+1, j-1]
            
            if magn[i, j] >= q and magn[i, j] >= r:
                res[i, j] = magn[i, j]
            else:
                res[i, j] = 0
    return res


def canny(sigma, thr_high, thr_low, img):
    nms = nonmax(sigma, img)
    gmax = np.max(nms)
    thr_high = gmax * thr_high
    thr_low = gmax * thr_low
    strong_x, strong_y = np.where((nms >= thr_high))
    weak_x, weak_y = np.where((nms >= thr_low) & (nms < thr_high))
    res = np.zeros_like(nms, dtype=np.uint8)
    res[strong_x, strong_y] = 255
    res[weak_x, weak_y] = 1
    neighbors = [(1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (1, -1), (-1, 1), (-1, -1)]
    
    x = list(strong_x)
    y = list(strong_y)
    while x:
        cx = x.pop(0)
        cy = y.pop(0)
        for dx, dy in neighbors:
            nx, ny = cx + dx, cy + dy
            if 0 <= nx < img.shape[1] and 0 <= ny < img.shape[0]:
                if res[nx, ny] == 1:
                    res[nx, ny] = 255
                    x.append(nx)
                    y.append(ny)
    res[res != 255] = 0
    return res


if __name__ == '__main__':
    parser = argparse.ArgumentParser(
        prog='ProgramName',
        description='What the program does',
        epilog='Text at the bottom of help',
    )
    parser.add_argument('command', help='Command description')
    parser.add_argument('parameters', nargs='*')
    parser.add_argument('input_file')
    parser.add_argument('output_file')
    args = parser.parse_args()

    img = skimage.io.imread(args.input_file)
    # img = img / 255
    if len(img.shape) == 3:
        img = img[:, :, 0]

    if args.command == 'grad':
        res, _ = grad(float(args.parameters[0]), img)
        res = np.round(res).astype(np.uint8)

    elif args.command == 'nonmax':
        res = nonmax(float(args.parameters[0]), img)
        res = np.round(res).astype(np.uint8)

    elif args.command == 'canny':
        res = canny(float(args.parameters[0]), float(args.parameters[1]), float(args.parameters[2]), img)
        res = np.round(res).astype(np.uint8)


    # res = np.clip(res, 0, 1)
    # res = np.round(res * 255).astype(np.uint8)
    skimage.io.imsave(args.output_file, np.clip(res, 0, 255))