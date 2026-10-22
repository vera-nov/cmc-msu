import argparse
import numpy as np
import skimage.io
from scipy.fft import fft2
from skimage.transform import warp_polar
from skimage.filters import window


def mse(img1, img2):
    return float(np.mean((img1.astype('float') - img2.astype('float')) ** 2))


def psnr(img1, img2):
    m = mse(img1, img2)
    if m == 0:
        return 'Images are identical'
    return 10 * np.log10(255 * 255 / m)


def ssim(img1, img2):
    c1 = 6.5025
    c2 = 58.5225
    mu1 = np.mean(img1)
    mu2 = np.mean(img2)
    sigma1_sq = np.var(img1)
    sigma2_sq = np.var(img2)
    sigma12 = np.mean((img1 - mu1) * (img2 - mu2))
    
    return ((2 * mu1 * mu2 + c1) * (2 * sigma12 + c2)) / ((mu1 ** 2 + mu2 ** 2 + c1) * (sigma1_sq + sigma2_sq + c2))


def median(rad, img):
    rad = int(rad)
    h, w = img.shape
    res = np.zeros_like(img)
    
    pad_width = rad
    padded_img = np.pad(img, pad_width, mode='edge')

    for i in range(h):
        for j in range(w):
            region = padded_img[i:i + 2 * rad + 1, j:j + 2 * rad + 1]
            res[i, j] = np.median(region)

    return res


def gauss(sigma_d, img):
    radius = int(3 * sigma_d)
    padded_img = np.pad(img, pad_width=radius, mode='edge')
    h, w = img.shape
    res = np.zeros_like(img)

    # посчитаем ядро
    x = np.linspace(-radius, radius, 2 * radius + 1)
    gauss_kernel = np.exp(-0.5 * (x / sigma_d) ** 2)
    gauss_kernel /= gauss_kernel.sum()

    for i in range(h):
        for j in range(w):
            region = padded_img[i:i + 2 * radius + 1, j:j + 2 * radius + 1]
            res[i, j] = np.sum(region * gauss_kernel[:, np.newaxis] * gauss_kernel[np.newaxis, :])
    
    return res


def bilateral(sigma_d, sigma_r, img):
    radius = int(3 * sigma_d)
    padded_img = np.pad(img, pad_width=radius, mode='edge')
    h, w = img.shape
    res = np.zeros_like(img)

    # посчитаем ядро
    k = np.arange(-radius, radius + 1)
    w1_1d = np.exp(-(k ** 2) / (2 * sigma_d ** 2))
    w1_1d /= w1_1d.sum()
    w1 = np.outer(w1_1d, w1_1d)
    int_range = np.arange(0, 256)
    w2 = np.exp(-(int_range[:, None] - int_range[None, :])**2 / (2 * sigma_r**2))

    for i in range(h):
        for j in range(w):
            region = padded_img[i:i + 2 * radius + 1, j:j + 2 * radius + 1]
            pixel = img[i, j]
            int_weights = w2[pixel, region]
            weights = w1 * int_weights
            res[i, j] = (region * weights).sum() / weights.sum()

    return res


def compare(img1, img2):
    def get_spectrum(img):
        img = img.astype(np.float64) / 255.0
        h, w = img.shape

        # преобразование фурье + амплитуда
        spectrum = np.abs(fft2(img))
        spectrum = np.fft.fftshift(spectrum)

        # небольшое размытие против алиасинга
        spectrum = gauss(0.5, spectrum)

        # полярные координаты
        radius = min(h, w) // 2
        polar = warp_polar(
            spectrum,
            center=(h // 2, w // 2),
            radius=radius,
            output_shape=(360, radius),
            scaling='linear'
        )
        # фурье вдоль угловой оси
        angular_fft = np.abs(np.fft.fft(polar, axis=0))
        return angular_fft

    spectrum1 = get_spectrum(img1)
    spectrum2 = get_spectrum(img2)

    energy1 = np.mean(spectrum1 ** 2)
    energy2 = np.mean(spectrum2 ** 2)
    energy = (energy1 + energy2) / 2
    if energy == 0:
        return int(np.array_equal(img1, img2))
    
    error = mse(spectrum1, spectrum2) / energy
    # print(error)

    threshold = 0.02
    return int(error < threshold)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(
        prog='ProgramName',
        description='What the program does',
        epilog='Text at the bottom of help',
    )
    parser.add_argument('command', help='Command description')
    parser.add_argument('parameters', nargs='*')
    args = parser.parse_args()

    if args.command in ['mse', 'psnr', 'ssim', 'compare']:
        input_file_1 = skimage.io.imread(args.parameters[0])
        if len(input_file_1.shape) == 3:
            input_file_1 = input_file_1[:, :, 0]
        
        input_file_2 = skimage.io.imread(args.parameters[1])
        if len(input_file_2.shape) == 3:
            input_file_2 = input_file_2[:, :, 0]
        
        func = globals()[args.command]
        res = func(input_file_1, input_file_2)
        print(res)

    elif args.command in ['median', 'gauss']:
        input_file = skimage.io.imread(args.parameters[1])
        input_file = input_file / 255
        if len(input_file.shape) == 3:
            input_file = input_file[:, :, 0]
        func = globals()[args.command]
        res = func(float(args.parameters[0]), input_file)
        res = np.clip(res, 0, 1)
        res = np.round(res * 255).astype(np.uint8)
        skimage.io.imsave(args.parameters[2], res)

    elif args.command == 'bilateral':
        input_file = skimage.io.imread(args.parameters[2])
        if len(input_file.shape) == 3:
            input_file = input_file[:, :, 0]
        res = bilateral(float(args.parameters[0]), float(args.parameters[1]), input_file)
        res = np.clip(res, 0, 255)
        res = np.round(res).astype(np.uint8)
        skimage.io.imsave(args.parameters[3], res)

    else:
        print('Unknown command')
