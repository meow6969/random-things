import math
import copy


class EpicNumber:
    def __init__(self, num, num_type):
        self.int: int = 0
        self.bits1: list = []
        self.bits8: list = []
        self.bytes: bytes = b""
        self.hex: str = ""

        self.update_num(num, num_type)

    def update_num(self, num, num_type):
        if num_type == "int":      # 35
            self.int = num
        elif num_type == "bits1":  # [0, 0, 0, 0, 0, 1, 1, 0] len() is always multiple of 8
            self.int = xbit_unsigned_int_to_int(num, bits=1)
        elif num_type == "bits8":  # [255, 0, 3, 2]
            self.int = xbit_unsigned_int_to_int(num)
        elif num_type == "bytes":  # b"\x00\xff\x45\xd4"
            self.int = bytes_to_int(num)
        elif num_type == "hex":    # "A45B06FF"
            self.int = xbit_unsigned_int_to_int(hexadecimal_string_to_ints(num))
        else:
            raise TypeError("number passed is not int, bits1, bits8, bytes, or hex.")
        self.populate_types()

    def populate_types(self):
        # print(self.int)
        self.bits1 = int_to_xbit_unsigned_ints(self.int, bits=1)
        self.bits8 = int_to_xbit_unsigned_ints(self.int)
        self.bytes = int_to_bytes(self.int)
        self.hex = int_to_hexadecimal_string(self.int)

    def print_values(self):
        print(f"int:    {self.int}")
        print(f"bits1:  {self.bits1}")
        print(f"bits8:  {self.bits8}")
        print(f"bytes:  {self.bytes}")
        print(f"hex:    {self.hex}")


def int_to_bytes(numbah):
    nums = int_to_xbit_unsigned_ints(numbah)

    byts = b""
    for num in nums:
        byts += int.to_bytes(num)
    return byts


def int_to_xbit_unsigned_ints(num, max_amt_ints=-1, fill_with_zeroes=True, bits=8):
    int_max = pow(2, bits)
    if num > pow(int_max, max_amt_ints) and max_amt_ints != -1:
        raise Exception("Number is too large for the defined amount of integers")
    required_amt_ints = 0
    while num > pow(int_max, required_amt_ints) - 1:
        required_amt_ints += 1

    ints = []
    for i in range(required_amt_ints):
        ints.append(0)

    if required_amt_ints <= 1:
        if fill_with_zeroes:
            for i in range(len(ints), max_amt_ints):
                ints.insert(0, 0)
            if num != 0:
                ints[-1] = num
            else:
                ints.append(0)
            if bits == 1 and fill_with_zeroes and len(ints) % 8 != 0:
                for i in range(8 - len(ints) % 8):
                    ints.insert(0, 0)
            return ints
        if bits == 1 and fill_with_zeroes and len(ints) % 8 != 0:
            return [0, 0, 0, 0, 0, 0, 0, num]
        return [num]
    required_amt_ints -= 1
    tha_num = num
    iterations = 1
    while required_amt_ints >= 0:
        times_divisible = math.floor(tha_num / pow(int_max, required_amt_ints))
        tha_num = tha_num % pow(int_max, required_amt_ints)
        ints[iterations - 1] = times_divisible
        required_amt_ints -= 1
        iterations += 1

    if fill_with_zeroes:
        for i in range(len(ints), max_amt_ints):
            ints.insert(0, 0)
    if bits == 1 and fill_with_zeroes and len(ints) % 8 != 0:
        for i in range(8 - len(ints) % 8):
            ints.insert(0, 0)
    # print(f"num: {num}")
    # print(ints)
    return ints


def xbit_unsigned_int_to_int(nums, bits=8):
    power = len(nums) - 1
    base = pow(2, bits)
    total = 0
    for number in nums:
        total += pow(base, power) * number
        power -= 1
    return total


def xor(num1: EpicNumber, num2: EpicNumber):
    nya1: list = []
    nya2: list = []
    if len(num1.bits1) > len(num2.bits1) or len(num1.bits1) == len(num2.bits1):
        nya1 = num1.bits1
        nya2 = num2.bits1
    elif len(num1.bits1) < len(num2.bits1):
        nya1 = num2.bits1
        nya2 = num1.bits1
    if len(nya1) == 0:
        nya1 = [0]
    if len(nya2) == 0:
        nya2 = [0]

    new_int = []
    nya2_index = 0
    for i in range(len(nya1)):
        if nya2_index == len(nya2):
            nya2_index = 0
        new_int.append(int(nya1[i] != nya2[nya2_index]))
        nya2_index += 1
    return EpicNumber(new_int, "bits1")


def hexadecimal_string_to_ints(txt: str):
    if len(txt) % 2 != 0:
        raise Exception("txt parameter must be even in length")

    txt = txt.lower()

    switch = {
        "0": 0,
        "1": 1,
        "2": 2,
        "3": 3,
        "4": 4,
        "5": 5,
        "6": 6,
        "7": 7,
        "8": 8,
        "9": 9,
        "a": 10,
        "b": 11,
        "c": 12,
        "d": 13,
        "e": 14,
        "f": 15
    }

    i = 0
    nums = []
    while len(txt) > i:
        nums.append(switch[txt[i]] * 16 + switch[txt[i + 1]])
        i += 2
    return nums


def int_to_hexadecimal_string(num):
    nums = int_to_xbit_unsigned_ints(num, bits=4)

    switch = {
        0: "0",
        1: "1",
        2: "2",
        3: "3",
        4: "4",
        5: "5",
        6: "6",
        7: "7",
        8: "8",
        9: "9",
        10: "a",
        11: "b",
        12: "c",
        13: "d",
        14: "e",
        15: "f"
    }

    if len(nums) % 2 == 1:
        nums.insert(0, 0)
    i = 0
    txt = ""
    while len(nums) > i:
        txt += switch[nums[i]] + switch[nums[i + 1]]
        i += 2
    return txt


def bytes_to_int(byts):
    numbas = []
    for byt in byts:
        numbas.append(byt)
    return xbit_unsigned_int_to_int(numbas)


def ascii_string_to_8bit(meow):
    nyas = []
    for char in meow:
        if ord(char) > 255:
            raise ValueError("character is not ascii")
        nyas.append(ord(char))
    return nyas


def gcd(numba1, numba2):
    if numba1 > numba2:
        num1 = numba1
        num2 = numba2
    else:
        num1 = numba2
        num2 = numba1

    found = False
    while not found:
        mult = 0
        while num1 - (mult * num2) >= num2:
            mult += 1
        if num1 - (mult * num2) == 0:
            found = True
        else:
            numb1 = num2
            numb2 = num1 - (mult * num2)
            num1 = numb1
            num2 = numb2

    return num2


def extended_gcd(numba1, numba2):  # taken straight from the wikipedia page for extended euclidean algorithm
    if numba1 > numba2:
        num1 = numba1
        num2 = numba2
    else:
        num1 = numba2
        num2 = numba1
    old_r = num1
    r = num2
    old_s = 1
    s = 0
    old_t = 0
    t = 1
    while r != 0:
        quotient = int(old_r / r)
        old_r, r = (r, old_r - quotient * r)
        old_s, s = (s, old_s - quotient * s)
        old_t, t = (t, old_t - quotient * t)
    print(f"Bézout coefficients: {old_s}, {old_t}")
    print(f"greatest common divisor: {old_r}")
    print(f"quotients by the gcd: {t}, {s}")
    return (old_s, old_t), old_r, (t, s)


if __name__ == "__main__":
    # print(int_to_hexadecimal_string(267))
    # print(bytes_to_int(b"\x02\x03"))
    # meow1 = EpicNumber([1, 0, 0, 0, 0, 0, 0, 0, 1, 1], "bits1")
    # meow2 = EpicNumber([1, 1, 0, 0, 1], "bits1")
    # meow3 = EpicNumber([0], "bits1")
    # meow4 = EpicNumber([1], "bits1")
    # meow1.print_values()
    # meow2.print_values()
    # meow3.print_values()
    # meow4.print_values()
    print(gcd(1071, 462))
    print(gcd(66528, 52920))
    print(gcd(240, 46))
    extended_gcd(240, 46)

