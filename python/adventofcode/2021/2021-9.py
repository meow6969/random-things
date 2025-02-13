# read and format input 
longtext = open('longtext.txt', 'r').read()
longtext = longtext.split('\n')

# function that displays the grid to the user
def display_grid(grid):
    string = ""
    strings = []
    for i in range(len(grid)):
        strings.append("")
    for i in grid:
        count = 0
        for y in i:
            strings[count] += str(y)  
            count += 1
        # string += "\n"
    for i in strings:
        if i.strip() != "":
            string += f"{i}\n"
    return string

# making the grid
grid = []
for i in range(len(longtext[0])):
    grid.append([])
count = 0
for i in longtext:
    count = 0
    for y in i:
        grid[count].append(int(y))
        count += 1
# grid = []
# for index,line in enumerate(longtext):
#     temp = []
#     for index2,number in enumerate(line):
#         temp.append(int(number))
#     grid.append(temp)
# displaying the newly made grid
# print(display_grid(grid))
# print(grid[3][4])

# a list made to keep track of all numbers that fit criteria
winners = []

# define the variables that keep track of what index the script is at
x_count = 0
y_count = 0

# loop through all X columns in the grid 2D list
for x in grid:
    # reset the y_count variable
    y_count = 0
    for y in x:
        # set the variables that will keep track of how many adjacent numbers are bigger
        top = False
        bottom = False
        left = False
        right = False
        
        # check if the number above y is bigger
        try:
            if grid[x_count][y_count - 1] > y:
            # if grid[y_count][x_count + 1] > y:
                top = True
        except IndexError:
            top = True
        
        # check if the number below y is bigger
        try:
            if grid[x_count][y_count + 1] > y:
            # if grid[y_count][x_count - 1] > y:
                bottom = True
        except IndexError:
            bottom = True
        
        # check if the number left of y is bigger
        try:
            if grid[x_count - 1][y_count] > y:
            # if grid[y_count][x_count + 1] > y:
                left = True
        except IndexError:
            left = True
        
        # check if the number right of y is bigger
        try:
            if grid[x_count + 1][y_count] > y:
            # if grid[y_count][x_count - 1] > y:
                right = True
        except IndexError:
            right = True
        
        # finally take all of those outputs and checks if they are all true
        if top and bottom and left and right:
            # if all true append the number to winners
            winners.append(grid[x_count][y_count])
        y_count += 1 
    x_count += 1
        
# print(winners)
# print(grid)
# for i in grid:
#     print(len(i))
# print(len(grid))
score = 0
for i in winners:
    # print(i + 1)
    score += (i + 1)
print(score)

    
