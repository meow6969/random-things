import utils

tha_meow = "label"

tha_num = utils.EpicNumber(utils.ascii_string_to_8bit(tha_meow), "bits8")

bleep = utils.xor(tha_num, utils.EpicNumber(13, "int"))

print(bleep.bytes.decode('utf-8'))

