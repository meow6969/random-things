data = open("input.txt").read()
"""
A = Rock (opponent)
B = Paper (opponent)
C = Scissors (opponent)

X = Rock
Y = Paper
Z = Scissors
rock = 1; paper = 2; scissors = 3;
loss = 0; draw = 3; win = 6;
"""

switch_pt1 = {
    "AX": 3 + 1,
    "AY": 6 + 2,
    "AZ": 0 + 3,

    "BX": 0 + 1,
    "BY": 3 + 2,
    "BZ": 6 + 3,

    "CX": 6 + 1,
    "CY": 0 + 2,
    "CZ": 3 + 3
}

"""
X = Lose
Y = Draw
Z = Win
Rock = 1; paper = 2; scissors = 3;
loss = 0; draw = 3; win = 6;
"""

switch_pt2 = {
    "AX": 0 + 3,
    "AY": 3 + 1,
    "AZ": 6 + 2,

    "BX": 0 + 1,
    "BY": 3 + 2,
    "BZ": 6 + 3,

    "CX": 0 + 2,
    "CY": 3 + 3,
    "CZ": 6 + 1
}

score_pt1 = 0
score_pt2 = 0
data = data.split("\n")
# print(data)
for line in data:
    # print(line)
    score_pt1 += switch_pt1[f"{line[0]}{line[2]}"]
    score_pt2 += switch_pt2[f"{line[0]}{line[2]}"]
print(score_pt1)
print(score_pt2)
