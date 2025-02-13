long_text = open("data.txt")


def challenge_1(text):
    first_num = 69
    last_num = 0
    nums = []

    for line in text.split("\n"):

        loops = 0
        for char in line:
            try:
                if first_num == 69:
                    first_num = int(char)
                else:
                    last_num = int(char)
                loops += 1
            except ValueError:
                pass
        if loops == 1:
            last_num = first_num
        if loops > 0:
            nums.append(int(str(first_num) + str(last_num)))
        first_num = 69
    # print(nums)
    x = 0
    for i in nums:
        x += i
    print(f"Challenge 1: {x}")


def challenge_2(text):
    rep = {
        "zero": "0",
        "one": "1",
        "two": "2",
        "three": "3",
        "four": "4",
        "five": "5",
        "six": "6",
        "seven": "7",
        "eight": "8",
        "nine": "9"
    }
    nums = []
    numsofnums = []
    for line in text.split("\n"):
        word = ""
        numerraa = []
        for char in line:
            try:
                numerraa.append(int(char))
                word = ""
            except ValueError:
                word += char
                for numbera in rep.keys():
                    if numbera in word:
                        numerraa.append(int(rep[numbera]))
                        word = word[word.find(numbera) + 1:]
        numsofnums.append(numerraa)

        nums.append(int(str(numerraa[0]) + str(numerraa[-1])))
    # matwiojn = text.split("\n")
    # for i, nn in enumerate(nums):
    #     # if len(numsofnums[i]) <= 1:
    #     print(f"\n{matwiojn[i]}\n{numsofnums[i]}, {nn}")
    # print(nums)
    x = 0
    for i in nums:
        x += i
    print(f"Challenge 2: {x}")


long_text = long_text.read()
challenge_1(long_text)
challenge_2(long_text)
