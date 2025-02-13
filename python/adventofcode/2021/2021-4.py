import os


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


# i was too lazy to filter out the results from teh input txt
order = [57, 9, 8, 30, 40, 62, 24, 70, 54, 73, 12, 3, 71, 95, 58, 88, 23, 81, 53, 80, 22, 45, 98, 37, 18, 72, 14, 20,
         66, 0, 19, 31, 82, 34, 55, 29, 27, 96, 48, 28, 87, 83, 36, 26, 63, 21, 5, 46, 33, 86, 32, 56, 6, 38, 52, 16,
         41, 74, 99, 77, 13, 35, 65, 4, 78, 91, 90, 43, 1, 2, 64, 60, 94, 85, 61, 84, 42, 76, 68, 10, 49, 89, 11, 17,
         79, 69, 39, 50, 25, 51, 47, 93, 44, 92, 59, 75, 7, 97, 67, 15]

boards_raw = open('../input.txt').read()
# print(boards_raw)
boards_raw_list = boards_raw.split("\n")
boards = []

# get a 3 dimensional list of all boards

count = 1
temp_lst = []
for i in boards_raw_list:
    if count < 6:
        if i != "":
            dictionary = {}
            for x in i.split():
                dictionary[int(x)] = 0
            temp_lst.append(dictionary)
    else:
        boards.append(temp_lst)
        temp_lst = []
        count = 0
    count += 1
###

winners = []
called_nums = 0
for i in order:
    if called_nums > 0:
        final_str = ""
        os.system('clear')
        print("CURRENT BOARDS:")
        board_count = 0
        for u in boards:
            board_count += 1
            board_str = ""
            board_str += f"board #{board_count}\n"

            for iop in u:
                for iopo in iop:
                    if iopo < 10:
                        iopy = f"{iopo} "
                    else:
                        iopy = iopo
                    if iop[iopo] == 0:
                        board_str += f"{iopy} "
                    else:
                        board_str += f"{bcolors.OKCYAN}{iopy}{bcolors.ENDC} "

                board_str += "\n"

            # bingo winning logic
            # input(u)

            # check x
            for q in range(5):
                # print(u[q])
                # print(list(u[q])[3])
                key1 = list(u[q])[0]
                key2 = list(u[q])[1]
                key3 = list(u[q])[2]
                key4 = list(u[q])[3]
                key5 = list(u[q])[4]
                if u[q][key1] == 1 and u[q][key2] == 1 and u[q][key3] == 1 and u[q][key4] == 1 and u[q][key5] == 1:
                    if board_count not in winners:
                        if board_count != 69:
                            winners.append(board_count)

            # check y
            for q in range(5):
                key1 = list(u[0])[q]
                key2 = list(u[1])[q]
                key3 = list(u[2])[q]
                key4 = list(u[3])[q]
                key5 = list(u[4])[q]
                if u[0][key1] == 1 and u[1][key2] == 1 and u[2][key3] == 1 and u[3][key4] == 1 and u[4][key5] == 1:
                    if board_count not in winners:
                        if board_count != 69:
                            winners.append(board_count)
            # print(board_count)
            if board_count not in winners:
                final_str += board_str
                final_str += "\n\n"
            # print(board_count)

        # if board_count == 89:
        #     final_str += board_str
        #     final_str += "\n\n"

        print(final_str)
        input(f"current iterations: {called_nums}")

    z_count = 0
    for z in boards:
        y_count = 0
        for y in z:
            x_count = 0
            for x in y:
                if x == i:
                    key = list(boards[z_count][y_count])[x_count]
                    boards[z_count][y_count][key] += 1
                x_count += 1
            y_count += 1
        z_count += 1
    called_nums += 1
