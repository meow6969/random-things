long_text = open("longtext.txt")
long_text = long_text.read()
count = 0
index = 0
for i in long_text:
    index += 1
    if i == '(':
        count += 1 
    elif i == ')':
        count -= 1
    if count == -1:
        break
print(count)
print()
print(index)
