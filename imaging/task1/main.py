import argparse
import numpy as np
import skimage.io


def mirror(img, axis='h'):
    '''Отразить изображение'''
    height, width = img.shape[:2]
    if axis in ['h', 'v']:
        new_shape = (height, width)
    else:
        new_shape = (width, height)
        
    res = np.zeros(new_shape, dtype=float)
    if axis == 'h':
        for y in range(0, height):
            for x in range(0, width):
                res[height - y - 1, x] = img[y, x]
    elif axis == 'v':
        for y in range(0, height):
            for x in range(0, width):
                res[y, width - x - 1] = img[y, x]
    elif axis == 'd':
        for y in range(0, height):
            for x in range(0, width):
                res[x, y] = img[y, x]
    else:
        for y in range(0, height):
            for x in range(0, width):
                res[width - x - 1, height - y - 1] = img[y, x]

    return res


def extract(img, left_x, top_y, width, height: int):
    img_height, img_width = img.shape[:2]
    res = np.empty((height, width), dtype=float)
    for y in range(0, height):
        for x in range(0, width):
            if (0 <= top_y + y < img_height) and (0 <= left_x + x < img_width):
                res[y, x] = img[top_y + y, left_x + x]
            else:
                res[y, x] = 0
                
    return res


def rotate(img, direction, angle):
    angle = angle % 360
    if angle == 0:
        return img


    height, width = img.shape[:2]
    if angle == 180:
        new_shape = (height, width)
    else:
        new_shape = (width, height)

    res = np.zeros(new_shape, dtype=float)
    if angle == 180:
        for y in range(0, height):
            for x in range(0, width):
                res[height - y - 1, width - x - 1] = img[y, x]
    elif (angle == 90 and direction == 'cw') or (angle == 270 and direction == 'ccw'):
        for y in range(0, height):
            for x in range(0, width):
                res[x, height - y - 1] = img[y, x]
    else:
        for y in range(0, height):
            for x in range(0, width):
                res[width - x - 1, y] = img[y, x]

    return res


def autocontrast(img):
    res = np.zeros_like(img)

    mn = np.min(img) # darkest pixel
    mx = np.max(img) # brightest pixel
    k = 1 / (mx - mn)

    height, width = img.shape[:2]
    for y in range(0, height):
        for x in range(0, width):
            res[y, x] = k * (img[y, x] - mn)
                
    return res


def fixinterlace(img):
    res = img.copy()
    height, width = img.shape[:2]
    for y in range(0, height):
        for x in range(0, width):
            if y % 2:
                res[y, x] = img[y - 1, x]
            else:
                res[y, x] = img[y + 1, x]

    
    def calc_variation(img):
        var = 0
        for y in range(1, height):
            for x in range(0, width):
                var += abs(img[y, x] - img[y - 1, x])
        return var

    if calc_variation(img) < calc_variation(res):
        return img

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
    img = img / 255
    if len(img.shape) == 3:
        img = img[:, :, 0]

    if args.command == 'mirror':
        res = mirror(img, args.parameters[0])

    elif args.command == 'extract':
        left_x, top_y, width, height = [int(x) for x in args.parameters]
        res = extract(img, left_x, top_y, width, height)

    elif args.command == 'rotate':
        direction = args.parameters[0]
        angle = int(args.parameters[1])
        res = rotate(img, direction, angle)

    elif args.command == 'autocontrast':
        res = autocontrast(img)

    elif args.command == 'fixinterlace':
        res = fixinterlace(img)


    res = np.clip(res, 0, 1)
    res = np.round(res * 255).astype(np.uint8)
    skimage.io.imsave(args.output_file, res)