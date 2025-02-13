long_text = open("longtext.txt", "r")
long_text = long_text.read()
long_text = long_text.split(",")
fishs = []
for i in long_text:
    fishs.append(int(i))
# print(fishs)

days = 0

roc = []

# while days < 100:
#     days += 1
#     old_fishs = len(fishs)
#     new_fishs = []
#     for i in fishs:
#         i -= 1 
#         if i < 0:
#             new_fishs.append(8)
    #         i = 6
    #     new_fishs.append(i)
    # fishs = new_fishs
    # #if days > 99:
    # rate_of_change = len(fishs) - old_fishs
    # roc.append(rate_of_change)
    # print(f"the rate of change for day {days} is {rate_of_change}")
    # print()
    # # print(f"After {days} days there is currently {len(fishs)} fish")
    
zeroes = 0 
ones = 0 
twos = 0 
threes = 0 
fours = 0 
fives = 0 
sixes = 0 
sevens = 0 
eights = 0 
for i in fishs:
    if i == 0:
        zeroes += 1 
    elif i == 1:
        ones += 1 
    elif i == 2: 
        twos += 1 
    elif i == 3:
        threes += 1 
    elif i == 4:
        fours += 1 
    elif i == 5:
        fives += 1 
    elif i == 6:
        sixes += 1 
    elif i == 7:
        sevens += 1 
    elif i == 8:
        eights += 1

while True:
    days += 1 
    old_zeroes = zeroes 
    old_ones = ones
    old_twos = twos 
    old_threes = threes 
    old_fours = fours 
    old_fives = fives 
    old_sixes = sixes 
    old_sevens = sevens 
    old_eights = eights 
    
    zeroes = old_ones
    ones = old_twos 
    twos = old_threes
    threes = old_fours 
    fours = old_fives
    fives = old_sixes 
    sixes = old_sevens + old_zeroes
    sevens = old_eights 
    eights = old_zeroes
    if days > 255:
        print(f"day {days}")
        print(f"fishs: "
            f"0:{zeroes}, 1:{ones}, 2:{twos}, 3:{threes}, 4:{fours}, 5:{fives}, 6:{sixes}, 7:{sevens}, 8:{eights}")
        input(f"total fish: {zeroes + ones + twos + threes + fours + fives + sixes + sevens + eights}")
    # 6,0,6,4,5,6,0,1,1,2,6,0,1,1,1,2,2,3,3,4,6,7,8,8,8,8
    # 0:3, 1:5, 2:3, 3:2, 4:2, 5:1, 6:5, 7:1, 8:4
    
