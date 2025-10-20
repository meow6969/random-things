use std::env;
// use std::io;
use std::process;
use std::fmt;
use rand::Rng;


fn vec_to_str<T: fmt::Display>(nya: &Vec<T>) -> String{
    let mut r = String::from("(");
    for i in nya {
        r = format!("{r}\"{i}\", ");
    }
    if r.len() < 2 {
        return String::from("()");
    }
    r = r[..r.len() - 2].to_string();
    r.push_str(")");
    return r;
}


fn print_vec<T: fmt::Display>(nya: &Vec<T>) {
    let nya_str: String = vec_to_str(nya);
    println!("vec={nya_str}");
}

fn get_args() -> Vec<String> {
    let args: Vec<String> = env::args().collect();
    if args.len() < 2 {
        println!("need at least 1 program arg!");
        process::exit(1);
    }
    return args[1..].to_vec();
}


fn main() {
    // let args: Vec<String> = get_args();
    let args: Vec<String> = get_args();
    let mut num_usernames: u32 = 6;
    let mut min_additions: usize = 2;
    let mut max_additions: usize = 12;

    let mut nicknames: Vec<String> = Vec::new();
    let mut what_im_like: Vec<String> = Vec::new();
    let mut hobbies: Vec<String> = Vec::new();
    let mut things_i_like: Vec<String> = Vec::new();
    let mut important_words: Vec<String> = Vec::new();
    let mut nums_and_letters: Vec<String> = Vec::new();
    
    let default_switch: String = String::from("");
    let mut current_switch: String = String::from("");
    let mut switch_args: u32 = 0;
    for arg in &args {
        if current_switch != default_switch {
            if switch_args > 0 {
                match current_switch.as_str() {
                    "-nickname"       => nicknames.push(arg.to_string()),
                    "-whatimlike"     => what_im_like.push(arg.to_string()),
                    "-hobbies"        => hobbies.push(arg.to_string()),
                    "-thingsilike"    => things_i_like.push(arg.to_string()),
                    "-importantwords" => important_words.push(arg.to_string()),
                    "-numsandletters" => nums_and_letters.push(arg.to_string()),
                    "-numusernames"   => num_usernames = arg.trim().parse::<u32>().expect("switch \"{current_switch}\": arg must be number!"),
                    "-minadditions"   => min_additions = arg.trim().parse::<usize>().expect("switch \"{current_switch}\": arg must be number!"),
                    "-maxadditions"   => max_additions = arg.trim().parse::<usize>().expect("switch \"{current_switch}\": arg must be number!"),

                    _ => {
                        println!("invalid switch: \"{current_switch}\"");
                        process::exit(2);
                    }
                }
                switch_args -= 1;
                continue;
            }
            current_switch = default_switch.clone();
        }
        if switch_args > 0 {
            println!("invalid number of args ({switch_args}) for switch \"{current_switch}\"");
            process::exit(3);
        }

        match arg.as_str() {
            "-nickname"       => {
                current_switch = String::from("-nickname");
                switch_args = 1;
            }
            "-whatimlike"     => {
                current_switch = String::from("-whatimlike");
                switch_args = 1;
            }
            "-hobbies"        => {
                current_switch = String::from("-hobbies");
                switch_args = 1;
            }
            "-thingsilike"    => {
                current_switch = String::from("-thingsilike");
                switch_args = 1;
            }
            "-importantwords" => {
                current_switch = String::from("-importantwords");
                switch_args = 1;
            }
            "-numsandletters" => {
                current_switch = String::from("-numsandletters");
                switch_args = 1;
            }
            "-numusernames" => {
                current_switch = String::from("-numusernames");
                switch_args = 1;
            }
            "-minadditions" => {
                current_switch = String::from("-minadditions");
                switch_args = 1;
            }
            "-maxadditions" => {
                current_switch = String::from("-maxadditions");
                switch_args = 1;
            }

            _ => {
                println!("invalid switch: {arg}");
                process::exit(2);
            }
        }
    }
    if switch_args > 0 {
        println!("invalid number of args ({switch_args}) for switch \"{current_switch}\"");
        process::exit(3);
    }
    if min_additions >= max_additions {
        println!("-minadditions ({min_additions}) cannot be greater than or equal to -maxadditions ({max_additions})");
        process::exit(4);
    }

    print_vec(&args);
    println!("config: ");
    println!("nicknames={}", vec_to_str(&nicknames));
    println!("what_im_like={}", vec_to_str(&what_im_like));
    println!("hobbies={}", vec_to_str(&hobbies));
    println!("things_i_like={}", vec_to_str(&things_i_like));
    println!("important_words={}", vec_to_str(&important_words));
    println!("nums_and_letters={}", vec_to_str(&nums_and_letters));
    println!("num_usernames={num_usernames}");
    println!("min_additions={min_additions}");
    println!("max_additions={max_additions}");
    println!();
    
    // let data: Vec<Vec<String>> = vec![nicknames, what_im_like, hobbies, things_i_like, important_words, nums_and_letters];
    let data: Vec<Vec<String>> = vec![nicknames, what_im_like, hobbies, things_i_like, important_words];
    // let min_iters: usize = 2;
    for _ in 0..num_usernames {
        let mut username: String = String::from("");
        let iters: usize = rand::thread_rng().gen_range(min_additions..=max_additions);
        // println!("iters={iters}");
        for n in 0..iters {
            let i: usize = rand::thread_rng().gen_range(0..=data.len() - 1);
            // println!("data.i={i}");
            let data_type: &Vec<String> = &data[i];
            // print_vec(data_type);
            let type_index: usize = rand::thread_rng().gen_range(0..=data_type.len() - 1);
            // println!("type_index={type_index}");
            username.push_str(&data_type[type_index]);
            if n == iters - 1 {
                continue;
            }
            let b: usize = rand::thread_rng().gen_range(0..=1);
            if b == 1 {
                continue;
            }
            let i: usize = rand::thread_rng().gen_range(0..=nums_and_letters.len() - 1);
            // println!("i={i}");
            username.push_str(&nums_and_letters[i]);
        }
        println!("username={username}");
    }
}
