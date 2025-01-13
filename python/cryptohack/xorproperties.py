import utils

key1 = utils.EpicNumber("a6c8b6733c9b22de7bc0253266a3867df55acde8635e19c73313", "hex")
key1_xor_key2 = utils.EpicNumber("37dcb292030faa90d07eec17e3b1c6d8daf94c35d4c9191a5e1e", "hex")
key2_xor_key3 = utils.EpicNumber("c1545756687e7573db23aa1c3452a098b71a7fbf0fddddde5fc1", "hex")
flag_xor_key1_xor_key2_xor_key3 = utils.EpicNumber("04ee9855208a2cd59091d04767ae47963170d1660df7f56f5faf",
                                                   "hex")
key2 = utils.xor(key1_xor_key2, key1)
key3 = utils.xor(key2_xor_key3, key2)

print(key1.int, key1.hex)
print(key2.int, key2.hex)
print(key3.int, key3.hex)

utils.xor(utils.xor(utils.xor(flag_xor_key1_xor_key2_xor_key3, key1), key2), key3).print_values()


