data = open("input.txt").read().split("\n")

def turn_to_list(thingy):
    pass

score_pt1 = 0
score_pt2 = 0

for pair in data:
    p1, p2 = pair.split(',')
    # print(p1)
    # print(p2)
    # print()
    p1 = p1.split("-")
    p2 = p2.split("-")
    # print(p1)
    # print(p2)
    # print()
    if int(p1[0]) <= int(p2[0]) and int(p1[1]) >= int(p2[1]) or \
        int(p2[0]) <= int(p1[0]) and int(p2[1]) >= int(p1[1]):
        # print(p1)
        # print(p2)
        score_pt1 += 1
    if int(p1[1]) >= int(p2[0]) and int(p1[0]) <= int(p2[0]) or \
        int(p1[1]) >= int(p2[0]) and int(p1[0]) <= int(p2[1]):
        # print(p1[1]) 
        # print(p2)
        # print()
        score_pt2 += 1
    

print(score_pt1)
print(score_pt2)
    
