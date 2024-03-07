# Safex Usage Guide

Safex provides a safe way to do the equivalent of bash parameter expansion without the risk of undesired interpretation by the shell of string contents.

## Features

- Safex allows literals to be injected into literals, avoiding the need for double quoting. This reduces the need to escape and take other convoluted precautions in order to protect text from interference by the shell. "Safe" in this context means 'safe from interpretation' by the shell causing wayward expansions, variable and command substitutions, globbing, brace expansions, tilde expansions, case modification, etc.

- It makes string manipulation and parameter passing more similar to the dominant paradigm used in programming languages, following the familiar pattern:
  
  `<Template with Embedded Arguments Placeholders {1} {2} {3}>`, 
  `<Value of Argument {1}>`, 
  `<Value of {2}>`, 
  `<Value of {3}>`... 

  This pattern is seen in C#'s `Console.WriteLine` and is common to many programming languages.

- Safex can be used to rearrange the order of parameters to allow more frequent use of bash aliases, especially when expressions require that the text or parameters provided after an alias (i.e., `myalias <myarg> <mytext>`) be located somewhere else than at the very end of the line.

- Note that more recent versions of Safex can return an `eval` command string that is executed by a bash wrapper function within a bash script. This is quite powerful, though clearly not at all 'safe'! Much of the documentation herein needs to be updated to reflect this change, which should be controlled by a command line switch to make the behaviour optional.

## Usage

```bash
safex templateString value1 [value2...]
```

## Examples

**Example with two parameters and special characters:**

```bash
output=$(./safex $'My dog\'s name is {0} and my cat\'s name is {1}' 'Fido' 'Whiskers')
eval "$output"
```

**Example with three parameters, special characters, and the need for complex escaping:**

```bash
output=$(./safex $'Here are the names: {0}, {1}, and {2}' 'Fido' 'Whiskers' 'Tweety')
eval "$output"` 
```

**Example using variables:**

```bash 
output=$(./safex "$template" "$param1" "$param2")
eval "$output"` 
```

**To execute the returned string (not necessary since recent changes):**

```bash
local temp_file=$(mktemp)
/usr/bin/safex "${args[@]}" > "$temp_file"
echo -en "executing: "; cat "$temp_file"; echo
sudo /usr/bin/unbuffer bash -c 'USER=root; ' "$(cat "$temp_file")" | more` 
```

## License

The code in this project is licensed under the MIT License. This means you can freely use, modify, distribute, and sell it under the same license.

```markdown
MIT License

Copyright (c) [year] [full name of copyright owner]

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## Disclaimer and Limitation of Liability

```markdown
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS, COPYRIGHT HOLDERS, OR PROJECT CONTRIBUTORS BE LIABLE FOR ANY CLAIM, DAMAGES, OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS RELATED TO THE SOFTWARE.

USE OF THE SOFTWARE IS AT YOUR OWN RISK. PLEASE REVIEW THE LICENSE AND THE SOFTWARE'S DOCUMENTATION BEFORE USING THE SOFTWARE. THE AUTHORS AND CONTRIBUTORS MAKE NO REPRESENTATIONS ABOUT THE SUITABILITY OF THIS SOFTWARE FOR ANY PURPOSE. 
Note: Replace [year] with the current year and [full name of copyright owner] with the name of the copyright holder (which might be your name or your organization's name). This text ensures users are aware of their rights under the MIT License, and the disclaimer provides a basic waiver of liability, common in open source projects.
```
