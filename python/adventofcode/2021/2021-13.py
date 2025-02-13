def output_grid(grid_, fancy=True):
    # count = 0
    strings = []
    for index in range(len(grid_)):
        if fancy:
            if index > 999:
                strings.append(f"{index} ")
            elif index > 99:
                strings.append(f"{index}  ")
            elif index > 9:
                strings.append(f"{index} ")
            else:
                strings.append(f"{index}  ")
        else:
            strings.append("")
    for index in grid_:
        # temp_string = ""
        # input(index)
        for y_val in range(len(index)):
            # input(list(index))
            # print(y_val)
            # print(list(index)[y_val])
            # input(index[])
            key = list(index)[y_val]
            value = index[key]
            if value == 0:
                value = '.'
            elif value > 0:
                value = '#'
            # temp_string += str(value)
            if fancy:
                strings[y_val] += str(value) + "  "
            else:
                strings[y_val] += str(value)
        # string += f"{temp_string}\n"
        # count += 1
    string = ""
    uhh = "   "
    for h in range(15):
        if fancy:
            if h < 10:
                uhh += str(h) + "  "
            else:
                uhh += str(h) + " "
    string += f"{uhh}\n"
    for index in strings:
        string += f"{index}\n"
    return string


def create_grid(x_, y_):
    grid = []
    for x in range(x_):
        pixel_row = {}
        for y in range(y_):
            pixel_row[y] = 0
        grid.append(pixel_row)
    return grid

longtext = open('longtext.txt', 'r').read()

longtext = longtext.split("\n")

instructions = []
fold_instructions = []

for line in longtext:
    if line == "":
        pass
    elif line[0] != "f":
        # print(line)
        line = line.split(',')
        instructions.append((int(line[0]), int(line[1])))
    else:
        line = line.replace('fold along ', '')
        fold_instructions.append(line)
# print(instructions)
# print(fold_instructions)


# print(x1_vals)
# print(y1_vals)
# input()

# create the grid
grid = create_grid(15, 15)



for i in instructions:
    x, y = i 
    grid[x][y] += 1

# print(output_grid(grid))

# for i in fold_instructions:
#     axis = i[0]
#     number = i[2]
#     if axis == 'x':
#         new_grid
#         grid_length = int(len(grid) / 2)
#         for count, x in enumerate(grid):
            
        
    
    
# print(output_grid(grid, False))








