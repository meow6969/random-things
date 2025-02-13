# read and format input 
longtext = open('longtext.txt', 'r').read()
longtext = longtext.split('\n')

valid = ["(", "[", "{", "<"]

switch = {
    "(": ")",
    "[": "]",
    "{": "}",
    "<": ">"
}

switch2 = {
    ")": 0,
    "]": 0,
    "}": 0,
    ">": 0
}

switch3 = {
    ")": 1,
    "]": 2,
    "}": 3,
    ">": 4
}

first = True

looking_for = []

valid_lines = []


# finding incorrect lines
for i in longtext:
    not_valid = False
    for a in i:
        
        if first:
            first = False
            looking_for.append(switch[a])
        else:
            if a in valid:
                looking_for.append(switch[a])
            else:
                b = len(looking_for)
                if looking_for[b - 1] == a:
                    expected = looking_for[b - 1]
                    del looking_for[b - 1]
                else:
                    # print(f"{i} - expected {looking_for[b - 1]}, but found {a} instead")
                    switch2[a] += 1
                    not_valid = True
                    break
    if not not_valid:
        valid_lines.append(i)

# completing valid lines 
added_chars = []
# print(valid_lines)
for i in valid_lines:
    looking_for = []
    for a in i:
        if first:
            first = False
            looking_for.append(switch[a])
        else:
            if a in valid:
                looking_for.append(switch[a])
            else:
                b = len(looking_for)
                if looking_for[b - 1] == a:
                    del looking_for[b - 1]
    looking_for.reverse()
    added_chars.append(looking_for)


# counting score
score = 0
score += 3 * switch2[")"]
score += 57 * switch2["]"]
score += 1197 * switch2["}"]
score += 25137 * switch2[">"]

print(score)

scores = []

for i in added_chars:
    temp_score = 0
    # print(''.join(i))
    for a in i:
        temp_score *= 5 
        temp_score += switch3[a]
    scores.append(temp_score)
scores.sort()
scores.reverse()
# print(scores)

print(scores[int(len(scores)/2)])

# for i in valid_lines:
#     print(i)
