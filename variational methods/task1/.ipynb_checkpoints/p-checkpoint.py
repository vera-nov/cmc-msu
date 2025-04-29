import numpy as np
from skimage import data
import numpy.core.multiarray
import matplotlib.pyplot as plt
from PIL import Image
from scipy.signal import convolve2d
import cv2

# TODO сделай import своей функции
from main import deconv

def mse(img1, img2):
    return float(np.mean((img1.astype('float') - img2.astype('float')) ** 2))


def psnr(img1, img2):
    m = mse(img1, img2)
    if m == 0:
        return 'Images are identical'
    return 10 * np.log10(255 * 255 / m)


images = (
    'astronaut',
    'brick',
    'camera',
    'cat',
    'checkerboard',
)


def testing_with_gauss_filter():
    noise_level = 0.1       # default

    for image in images:
        caller = getattr(data, image)
        reference = caller()
        if len(reference.shape) == 3:
            reference = reference[:, :, 0]

        size = np.random.randint(3, 9)

        # filter Gauss
        kernel = cv2.getGaussianKernel(ksize=size, sigma = np.random.randint(0, 20))
        kernel = kernel @ kernel.T
        kernel /= kernel.sum()


        blurred_image = convolve2d(reference, kernel, mode='same')

        Image.fromarray(reference.astype(np.uint8)).save(f"{image}_original.bmp")
        Image.fromarray(blurred_image.astype(np.uint8)).save(f"{image}_blur_gauss.bmp")
        print(kernel.shape)

        output = deconv(blurred_image, kernel, noise_level)

        Image.fromarray(output.astype(np.uint8)).save(f"{image}.bmp")

        assert (psnr(blurred_image, reference) < psnr(output, reference))


def testing_with_motion_blur():
    noise_level = 0.1       # default

    for image in images:
        caller = getattr(data, image)
        reference = caller()
        if len(reference.shape) == 3:
            reference = reference[:, :, 0]

        size = np.random.randint(5, 13)
        kernel = np.zeros((size, size))
        cv2.line(kernel, (0, 0), (size-1, size), 1, thickness=1)
        cv2.line(kernel, (1, 1), (3, 3), 1, thickness=1)
        cv2.line(kernel, (0, size-1), (size-1, 0), 1, thickness=1)
        kernel /= kernel.sum()

        blurred_image = convolve2d(reference, kernel, mode='same')

        Image.fromarray(reference.astype(np.uint8)).save(f"{image}_original.bmp")
        Image.fromarray(blurred_image.astype(np.uint8)).save(f"{image}_blur_motion.bmp")
        print(kernel.shape)

        output = deconv(blurred_image, kernel, noise_level)

        Image.fromarray(output.astype(np.uint8)).save(f"{image}.bmp")

        assert (psnr(blurred_image, reference) < psnr(output, reference))

