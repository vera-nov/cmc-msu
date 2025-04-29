import argparse
import numpy as np
import skimage.io


def mse(img1, img2):
    return np.mean((img1 - img2) ** 2)


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

    difference = np.abs(img1 - img2)
    
    # Find the maximum difference and its coordinates
    max_difference = np.max(difference)
    max_indices = np.unravel_index(np.argmax(difference), difference.shape)
    print(max_indices)
    
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
    k_range = np.arange(-radius, radius + 1)
    w1_1d = np.exp(-(k_range ** 2) / (2 * sigma_d ** 2))
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
    pass

def bilateral_kernel(img,size,sigma_d,sigma_r):
    lim=(size-1)/(2*sigma_d)
   # print(sigma)
    corex=np.linspace(-lim,lim,size)
    corex=np.exp(-0.5*corex*corex)
    corex_i=np.exp(-0.5*((img-img[size//2][size//2])/(sigma_r))**2)
    w=corex[None,:]*corex[:,None]*corex_i
    bilat=(np.sum(w*img))/(np.sum(w))


    return bilat

def fltr(image,kernel,size=1,/,**kwards):
    kernel_size=size
    #print(kwards)
    image1=np.copy(image).astype(np.float32)
    #print(image[:,[-1]])
    image1=np.insert(image1,[0]*(kernel_size//2),image1[:,[0]],axis=1)
    image1=np.insert(image1,[-1]*(kernel_size//2),image1[:,[-1]],axis=1)
    #print(np.concatenate(((image[:,[-1]],image1[:,[-1]])),axis=1))
    image1=np.insert(image1,[0]*(kernel_size//2),image1[[0]],axis=0)
    image1=np.insert(image1,[-1]*(kernel_size//2),image1[[-1]],axis=0)
    #print(image1[:,[-1]])
    h,w=image.shape

    filter_image=np.zeros(image.shape,dtype=np.float32)
    for i in range(h):
        for j in range(w):
            window=image1[i:i+kernel_size,j:j+kernel_size]
            filter_image[i][j]=kernel(window,size,**kwards)
    filter_image=(filter_image.clip(0,255)).astype(np.uint8)
    return filter_image


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
        # input_file_1 = input_file_1 / 255
        if len(input_file_1.shape) == 3:
            input_file_1 = input_file_1[:, :, 0]
        
        input_file_2 = skimage.io.imread(args.parameters[1])
        # input_file_2 = input_file_2 / 255
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
        res = fltr(input_file, bilateral_kernel, 3 * int(args.parameters[0]), sigma_d=int(args.parameters[0]), sigma_r=int(args.parameters[1]))
        skimage.io.imsave(args.parameters[3], res)

    else:
        print('Unknown command')
