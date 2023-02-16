levels = [
  "1s",
  "2s",
  "2p",
  "3s",
  "3p",
  "4s",
  "3d",
  "4p"
]

electron_holds = {
  "s": 2,
  "p": 6,
  "d": 10,
  "f": 14
}

atom = 33

orbital_filling_diagram = []

electrons = atom 
current_level = 0
current_levels_electrons = 0
while electrons > 0:
  # print(electron_holds[levels[current_level][-1:]])
  if current_levels_electrons >= electron_holds[levels[current_level][-1:]]:
    orbital_filling_diagram.append(current_levels_electrons)
    current_level += 1
    current_levels_electrons = 0
  else:
    current_levels_electrons += 1
    electrons -= 1

orbital_filling_diagram.append(current_levels_electrons)

human_readable_str = ""

for i, value in enumerate(orbital_filling_diagram):
  human_readable_str += f"{levels[i]}({value}) "
print(human_readable_str)
