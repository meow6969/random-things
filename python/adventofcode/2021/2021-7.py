longtext = open('longtext.txt').read()

longtext = longtext.split(",")

# dictionary = {}

# for i in longtext:
#     try:
#         dictionary[i] += 1 
#     except KeyError:
#         dictionary[i] = 1
# print(dictionary)
fuel = 0
number = 0
old_fuel = 0
meowing = True
while meowing:
    fuel = 0
    for i in longtext:
        gamer = 0
        
        # print(number)
        # input(gamer)
        # print(gamer)
        num = number - int(i) 
        gamer = 0
        for m in range(abs(num)):
            gamer += m + 1 
        # print('meow')
        fuel += gamer
    if number != 0:
        if old_fuel < fuel:
            meowing = False
            answer = number - 1
            fuel = old_fuel
        else:
            old_fuel = fuel
    
    else:
        old_fuel = fuel 
    print(fuel)
    number += 1 

# new_dictionary = dict(sorted(dictionary.items(), key=lambda item: item[1]))
# print(new_dictionary)
print(number)
print(fuel)
