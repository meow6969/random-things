def check_stuff(sacks):
    # print(sacks)
    for char in sacks[0]:
        if char in sacks[1] and char in sacks[2]:
            # print(char)
            to_add = switch[char.lower()]
            # score_pt2 += switch[char.lower()]
            if char.isupper():
                to_add += 26
            # print(to_add)
            break
    return to_add


data = open("input.txt").read().split("\n")

switch = {
    "a": 1,
    "b": 2,
    "c": 3,
    "d": 4,
    "e": 5,
    "f": 6,
    "g": 7,
    "h": 8,
    "i": 9,
    "j": 10,
    "k": 11,
    "l": 12,
    "m": 13,
    "n": 14,
    "o": 15,
    "p": 16,
    "q": 17,
    "r": 18,
    "s": 19,
    "t": 20,
    "u": 21,
    "v": 22,
    "w": 23,
    "x": 24,
    "y": 25,
    "z": 26
}

priorities_pt1 = {}
score_pt2 = 0
sacks = []
for sack in data:
    # print(len(sack))
    # print(sack)
    sack1 = sack[:int(len(sack) / 2)]
    sack2 = sack[int(len(sack) / 2):]
    # print(sack1)
    # print(sack2)
    counted = []
    for item in sack1:
        if item in sack2 and item not in counted:
            # print(item)
            priority = 0
            priority += switch[item.lower()]
            if item.isupper():
                priority += 26
            try:
                priorities_pt1[item] += priority
            except KeyError:
                priorities_pt1[item] = priority
            counted.append(item)

    sacks.append(sack)
    if len(sacks) == 3:
        score_pt2 += check_stuff(sacks)
        sacks = []

# score_pt2 += check_stuff(sacks)


# print(priorities)
score_pt1 = 0

for i in priorities_pt1:
    score_pt1 += priorities_pt1[i]
print(score_pt1)
print(score_pt2)