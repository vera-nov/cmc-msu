def find_modified_max_argmax(L, f):
    L = list(map(f, [i for i in L if type(i) is int]))
    return () if L == [] else (max(L), L.index(max(L)))