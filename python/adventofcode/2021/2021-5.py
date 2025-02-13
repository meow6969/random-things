# import pygame


def output_grid(grid_):
    # count = 0
    strings = []
    for index in range(len(grid_)):
        if index > 999:
            strings.append(f"{index} ")
        elif index > 99:
            strings.append(f"{index}  ")
        elif index > 9:
            strings.append(f"{index}   ")
        else:
            strings.append(f"{index}  ")
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
            # temp_string += str(value)
            strings[y_val] += str(value) + " "
        # string += f"{temp_string}\n"
        # count += 1
    string = ""
    uhh = "   "
    for h in range(10):
        uhh += str(h) + " "
    string += f"{uhh}\n"
    for index in strings:
        string += f"{index}\n"
    return string


longtext = open('longtext.txt').read()

longtext = longtext.split("\n")
x_vals = []
y_vals = []
for i in longtext:
    try:
        x, y = i.split('->')
        x_vals.append(x)
        y_vals.append(y)
    except ValueError:
        pass

# print(x_vals)
# print()
# print(y_vals)
x1_vals = []
y1_vals = []
for i in x_vals:
    x1, y1 = i.split(',')
    x1_vals.append(int(x1))
    y1_vals.append(int(y1))

x2_vals = []
y2_vals = []
for i in y_vals:
    x2, y2 = i.split(',')
    x2_vals.append(int(x2))
    y2_vals.append(int(y2))

# print(x1_vals)
# print(y1_vals)
# input()

# create the grid
grid = []
for x in range(1000):
    pixel_row = {}
    for y in range(1000):
        pixel_row[y] = 0
    grid.append(pixel_row)

# grid[0][0] += 1
# grid[9][9] += 1
# grid[9][0] += 1
# grid[0][9] += 1
# input(output_grid(grid))

for i in range(500):
    x1 = x1_vals[i]
    x2 = x2_vals[i]
    y1 = y1_vals[i]
    y2 = y2_vals[i]

    # print(i)
    if x1 == x2:
        # print(f"drawing diagonal line {x1},{y1}-->{x2},{y2}")
        y = y1 - y2
        negative = False
        if y < 0:
            negative = True
            # print('negative is true')
        y = abs(y) + 1

        # print(range(y))
        for m in range(y):
            if negative:
                grid[x1][y1 + m] += 1
            else:
                grid[x1][y1 - m] += 1
        # print(output_grid(grid))

    elif y1 == y2:
        # print('meow')
        # print(f"drawing horizontal line {x1},{y1}-->{x2},{y2}")
        x = x1 - x2
        negative = False
        if x < 0:
            negative = True
            # print('negative is true')

        x = abs(x) + 1
        # print(range(x))
        for m in range(x):
            # input(m)
            if negative:
                grid[x1 + m][y1] += 1
            else:
                # print(y1)
                # print(x1 + m)
                grid[x1 - m][y1] += 1
        # print(output_grid(grid))

    # if its a diagonal
    else:
        x = x1 - x2
        negative_x = False
        if x < 0:
            negative_x = True

        y = y1 - y2
        negative_y = False
        if y < 0:
            negative_y = True
        y = abs(y) + 1
        x = abs(x) + 1
        # print(range(x))
        for m in range(x):
            # input(m)
            if negative_x and not negative_y:
                # print(f"{x1},{y1}-->{x2},{y2}")
                # print(f"x and y values: {x},{y}")
                # print(f"dot supposed to be filled in: {x1 + m}, {y1 + m}")
                grid[x1 + m][y1 - m] += 1

            if negative_y and not negative_x:
                # print("negative_y ran")
                grid[x1 - m][y1 + m] += 1
            if negative_y and negative_x:
                # print("negative_y and negative_x ran")
                grid[x1 + m][y1 + m] += 1
            if not negative_x and not negative_y:
                # print("else ran")
                # print(y1)
                # print(x1 + m)
                grid[x1 - m][y1 - m] += 1
        # input(output_grid(grid))

    # print(output_grid(grid))

# grid[4][2] += 1
# print(output_grid(grid))

# output = output_grid(grid)
# output_txt = open("output.txt", "w+")
# output_txt.write(output)

overlap = 0

for i in grid:
    # temp_string = ""
    # input(index)
    for y_val in range(len(i)):
        # input(list(index))
        # print(y_val)
        # print(list(index)[y_val])
        # input(index[])
        key = list(i)[y_val]
        value = i[key]
        if value > 1:
            overlap += 1
print(overlap)   
