long_text = open("longtext.txt")
long_text = long_text.read()
long_text = long_text.split("\n")
x = 0
depth = 0
aim = 0
for i in long_text:
    instruction = i.split()
    instr_a = instruction[0]
    instr_b = instruction[1]
    if instr_a == "forward":
        x += int(instr_b)
        depth += int(instr_b) * aim
    elif instr_a == "down":
        aim += int(instr_b)
    elif instr_a == "up":
        aim -= int(instr_b)
    

print(f"{depth}x{x}")
print(depth*x)
    
