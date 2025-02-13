long_text = open("longtext.txt", "r")
long_text = long_text.read()
long_text = long_text.split("\n")
gamma = []
epsilon = []
#iters = 12 # amount of rows
#counts = 1000 # amount of columns
iters = 0
counts = 0 

for i in long_text:
    counts += 1
for i in long_text[0]:
    iters += 1 
#print(iters)
#input(counts)
# input(long_text)
completed_num = ""
completed_num_2 = ""
xd = 0

index = 0

finding = True
while finding:
    zeroes = []
    ones = []
    one_vals = 0 
    zero_vals = 0
    for y in range(len(long_text)):
        # print(xd)
        val = long_text[y][index]
        val = int(val)
        # print(val)
        if val == 0:
            zeroes.append(long_text[y])
            zero_vals += 1 
        if val == 1:
            ones.append(long_text[y])
            one_vals += 1
    
    
    if zero_vals > one_vals:
        completed_num += "0"
        completed_num_2 += "1"
        long_text = zeroes
    elif one_vals > zero_vals:
        completed_num += "1"
        completed_num_2 += "0"
        long_text = ones
    else:
        completed_num += "1"
        completed_num_2 += "0"
        long_text = ones
    print(ones)
    print()
    print(zeroes)
    print(completed_num)
    print(completed_num_2)
    print()
    print(int(completed_num, 2))
    input(int(completed_num_2, 2))
    
    index += 1
        
#print(gamma)
#print(epsilon)
#print("converted:")
#gamma_bin = ""
#for i in gamma: 
#    gamma_bin += str(i)
#gamma_bin = int(gamma_bin, 2)
#epsilon_bin = ""
#for i in epsilon:
#    epsilon_bin += str(i) 
#epsilon_bin = int(epsilon_bin, 2)
#print(f"EPSILON: {epsilon_bin}")
#print(f"GAMMA: {gamma_bin}")
#print(f"POWER CONSUMPTION: {epsilon_bin*gamma_bin}")
        
        
print(zeroes)
print()
print(ones)
        
        
        

        
        
        
