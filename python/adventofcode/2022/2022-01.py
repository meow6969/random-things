data = open("input.txt").read()

data_list = []
new_str = ""
for character in data:
    if character == "\n":
        data_list.append(new_str)
        new_str = ""
    else:
        new_str += character

data_list.append(new_str)
data = data_list
# print(data)
# data = data.split()

elves_calories = []
elf_num = 0
for line in data:
    # print(line)
    if line.strip() != '':
        try:
            elves_calories[elf_num] += int(line)
        except IndexError:
            elves_calories.append(int(line))
    else:
        elf_num += 1

elves_calories.sort()

print(elves_calories)
print(elves_calories[len(elves_calories) - 1] + elves_calories[len(elves_calories) - 2] + elves_calories[len(elves_calories) - 3])
# input()
