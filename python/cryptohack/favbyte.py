import utils

data = utils.EpicNumber("73626960647f6b206821204f21254f7d694f7624662065622127234f726927756d", "hex")

for i in range(256):
    mew = utils.EpicNumber(i, "int")
    nya_bytes = utils.xor(mew, data).bytes
    if nya_bytes.startswith(b"crypto{"):
        print(nya_bytes.decode("utf-8"))
