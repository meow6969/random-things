longtext = open('longtext.txt', 'r').read()

longtext = longtext.split("\n")


class bcolors:
    HEADER = '\033[95m'
    OKBLUE = '\033[94m'
    OKCYAN = '\033[96m'
    OKGREEN = '\033[92m'
    WARNING = '\033[93m'
    FAIL = '\033[91m'
    ENDC = '\033[0m'
    BOLD = '\033[1m'
    UNDERLINE = '\033[4m'


def output_grid(grid_, fancy=True):
    # count = 0
    string = ""
    for index in grid_:
        temp_string = ""
        # input(index)
        for y_val in range(len(index)):
            key = list(index)[y_val]
            value = index[key]
            # temp_string += str(value)
            if fancy:
                temp_string += str(value) + "  "
            else:
                temp_string += str(value)
        string += f"{temp_string}\n"
        # string += f"{temp_string}\n"
        # count += 1
    # string = ""
    # uhh = "   "
    # for h in range(10):
    #     if fancy:
    #         if h < 10:
    #             uhh += str(h) + "  "
    #         else:
    #             uhh += str(h) + " "
    # string += f"{uhh}\n"
    # for index in strings:
    #     string += f"{index}\n"
    return string


def create_grid(x_, y_, longtext):
    grid = []
    for x in range(x_):
        pixel_row = {}
        for y in range(y_):
            # print(longtext[x][y])
            pixel_row[y] = longtext[x][y]
        grid.append(pixel_row)
    return grid


grid = create_grid(len(longtext[0]), len(longtext), longtext)


grid[1][2] = f"{bcolors.BOLD}{grid[1][2]}{bcolors.ENDC}"

print(output_grid(grid, False))















