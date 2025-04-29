def check(x: str, file: str):
    f = open(file, "w")
    x = list(x.lower().split())
    unique = sorted(list(set(x)))
    for word in unique:
        f.write(word + " " + str(x.count(word)) + "\n")
    f.close()
