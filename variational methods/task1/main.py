import numpy as np
import argparse
import skimage.io
from scipy.signal import convolve2d
import math

def mse(img1, img2):
    return float(np.mean((img1.astype('float') - img2.astype('float')) ** 2))

def psnr(img1, img2):
    m = mse(img1, img2)
    if m == 0:
        return 'Images are identical'
    return 10 * np.log10(255 * 255 / m)

def shift(image, x_shift, y_shift):
    res = np.roll(np.pad(np.roll(np.pad(
        image, pad_width=((np.abs(x_shift), 0)), mode='edge'), x_shift, axis=1),
        pad_width=((0, np.abs(y_shift))), mode='edge'), y_shift, axis=0)
    start_x = np.abs(x_shift)
    end_x = res.shape[0] - np.abs(y_shift)
    start_y = np.abs(x_shift)
    end_y = res.shape[1] - np.abs(y_shift)
    return res[start_x:end_x, start_y:end_y]

def tgv(image, Q, noise_level):
    alpha1 = 0.1 * noise_level
    alpha2 = 0.05 * noise_level
    res = np.zeros_like(image, dtype=np.float64)
    
    for x, y in Q:
        shifted_xy = shift(image, x, y)
        sgn = np.sign(shifted_xy - image)
        sgn_coef = shift(sgn, -x, -y)
        res += alpha1 * (sgn_coef - sgn) / np.sqrt(x**2 + y**2)

    for x, y in Q:
        shifted_minusxy = shift(image, -x, -y)
        sgn = np.sign(shifted_xy + shifted_minusxy - 2 * image)
        sgn_coef = shift(sgn, -x, -y)
        res += alpha2 * (sgn_coef - sgn) / np.sqrt(x**2 + y**2)
        
    return res

def gradient(image, kernel, blurry_image, Q, noise_level):
    residual = convolve2d(image, kernel, mode='same', boundary='symm') - blurry_image
    gradient = convolve2d(residual, kernel[::-1, ::-1], mode='same', boundary='symm')
    gradient += tgv(image, Q, noise_level)
    return gradient

def deconv(input_image, kernel, noise_level):
    Q = [(1,0), (0,1), (1,1), (1,-1)]
    max_iter = 100
    mu = 0.85
    lr_init = 1

    image = np.copy(input_image).astype(np.float64)
    velocity = np.zeros_like(image)

    for i in range(max_iter):
        lr = lr_init * 0.5 * (1 + np.cos((i / max_iter) * np.pi))
        residual = np.sum(np.abs(convolve2d(image, kernel, mode='same', boundary='symm') - input_image))
        
        prev_image = np.copy(image)
        grad = gradient(prev_image + mu * velocity, kernel, input_image, Q, noise_level)
        velocity = mu * velocity - lr * grad
        image += velocity
        
        if i > 10 and residual > prev_residual:
            break
        prev_residual = residual

    return image

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('parameters', nargs='*')
    args = parser.parse_args()
    
    input_file = skimage.io.imread(args.parameters[0])
    if len(input_file.shape) == 3:
        input_file = input_file[:, :, 0]
        
    kernel = skimage.io.imread(args.parameters[1])
    if len(kernel.shape) == 3:
        kernel = kernel[:, :, 0]
    kernel = kernel.astype(np.float64) 
    kernel /= np.sum(kernel)
    
    noise_level = float(args.parameters[3])
    res = deconv(input_file, kernel, noise_level)
    res = np.clip(res, 0, 255)
    res = np.round(res).astype(np.uint8)
    skimage.io.imsave(args.parameters[2], res)

    '''
    orig_image = skimage.io.imread("reference.bmp")
    if len(orig_image.shape) == 3:
        orig_image = orig_image[:, :, 0]
    print("psnr blurred-original: ", psnr(input_file, orig_image))
    print("psnr mine-original: ", psnr(res, orig_image))
    '''