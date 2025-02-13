data = open("input.txt").read().split("\n")


for i, line in enumerate(data):
    if line[1].isdigit():
        counting_y = i
        break

mult = 4
pointer = 1
counting_x = 0
meowing = True
while meowing:
    try:
        if data[counting_y][pointer].isdigit():
            counting_x += 1 
        else:
            meowing = False
    except IndexError:
        meowing = False
    pointer += mult

crates = []

mult = 5
offset = 0
for x in range(counting_x):
    stack = []
    if x == 0:
        val = 1 
    else:
        val = x * mult - offset
        offset += 1
    for y in range(counting_y, -1, -1):
        if data[y][val].isalpha():
            stack.append(data[y][val])
    crates.append(stack)
print(crates)
    
# EVERYTHING ABOVE THIS CREATES A LIST    
