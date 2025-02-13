long_text = open("data.txt")


def challenge_1(text):
    found_num = False
    nums = []
    cur_nums = ""
    cur_num = {}
    symbols = []
    for y, line in enumerate(text.split("\n")):
        for x, char in enumerate(line):
            if char.isdigit():
                if not found_num:
                    cur_num = {
                        "start_coords": (x, y)
                    }
                    cur_nums = ""
                    found_num = True
                cur_nums += char
            elif found_num:
                cur_num["end_coords"] = (x - 1, y)
                cur_num["nums"] = cur_nums
                nums.append(cur_num)
                found_num = False
            if not char.isdigit() and char != ".":
                symbols.append((x, y))
        if found_num:
            cur_num["end_coords"] = (len(line), y)
            cur_num["nums"] = cur_nums
            nums.append(cur_num)
            found_num = False

    adjacent_nums = []
    for i in symbols:
        x, y = i
        for m in nums:
            for offset in range(-1, 2):
                if m["start_coords"][1] == y + offset:
                    if x - 1 <= m["start_coords"][0] <= x + 1 or x - 1 <= m["end_coords"][0] <= x + 1:
                        adjacent_nums.append(m)
    x = 0
    accounted_nums = []
    for i in adjacent_nums:
        if i not in accounted_nums:
            accounted_nums.append(i)
            x += int(i["nums"])
    print(f"Challenge 1: {x}")


def challenge_2(text):
    found_num = False
    nums = []
    cur_nums = ""
    cur_num = {}
    gears = []
    for y, line in enumerate(text.split("\n")):
        for x, char in enumerate(line):
            if char.isdigit():
                if not found_num:
                    cur_num = {
                        "start_coords": (x, y)
                    }
                    cur_nums = ""
                    found_num = True
                cur_nums += char
            elif found_num:
                cur_num["end_coords"] = (x - 1, y)
                cur_num["nums"] = cur_nums
                nums.append(cur_num)
                found_num = False
            if char == "*":
                gears.append((x, y))
        if found_num:
            cur_num["end_coords"] = (len(line), y)
            cur_num["nums"] = cur_nums
            nums.append(cur_num)
            found_num = False

    accounted_nums = []
    gear_ratios = []
    for i in gears:
        x, y = i
        gear_adjacents = []
        for m in nums:
            for offset in range(-1, 2):
                if m["start_coords"][1] == y + offset:
                    if x - 1 <= m["start_coords"][0] <= x + 1 or x - 1 <= m["end_coords"][0] <= x + 1:
                        if m not in accounted_nums:
                            gear_adjacents.append(m)
                            accounted_nums.append(m)
        if len(gear_adjacents) == 2:
            gear_ratios.append(int(gear_adjacents[0]["nums"]) * int(gear_adjacents[1]["nums"]))

    x = 0
    for i in gear_ratios:
        x += i

    print(f"Challenge 2: {x}")


long_text = long_text.read()
challenge_1(long_text)
challenge_2(long_text)

