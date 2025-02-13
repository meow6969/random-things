long_text = open("longtext.txt")
long_text = long_text.read()
b4 = 0
first = True
long_text = long_text.split()
anser = ""
count = 0
index = -1
for i in long_text:
    index += 1
    print(1)
    try:
        a = int(i)
        b = int(long_text[index+1])
        c = int(long_text[index+2])
    except IndexError:
        print('erer')
        break
    cur = a + b + c
    if first:
        b4 = cur
        first = False
    else:
        if cur > b4:
            count += 1
        b4 = cur
        

print(anser)
print()
print(count)
