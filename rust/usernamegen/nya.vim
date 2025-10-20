echo split("meow.meow", ".")

let f = expand("%:p")
let o = expand("%:e")
echo $"%={f}"
echo $"ext={o}"

