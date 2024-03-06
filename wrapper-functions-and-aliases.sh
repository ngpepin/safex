#!/bin/bash
#
# PURPOSE : Bash wrapper functions and alias examples for safex - should be put in ~/.bashrc
#
# EXAMPLES : 
#
# 1. Recursively lists the top newest files in reverse chronological order.
# Usage: lsnew
#
alias lnew='ssbxp "if [[ \"{1}\" == \"<nil>\" ]]; then myp1=\".\"; else myp1=\"{1}\"; fi; find \"\$myp1\" -maxdepth 3 -type f -printf \"%T+ %p\n\" | sort -r | head -n 100 | sed \"s/\.[^ ]* / /\""'

# 2. Provides compact and colourful list of processes that are listening to ports and what those ports are
# Usage: listen?
#
alias listen?="sudo ss -tuln4p | grep LISTEN --color=always | awk '{print \$5, \$1, \$7}' | sed 's/:/ /' | sort -n -k 2 | sed 's/users:(//g' | sed 's/)//g' | sed 's/\"//g' | sed 's/(//g' | awk '{printf \"%-20s %-10s \033[31m%-20s\033[0m \033[94m%s\033[0m\\n\", \$1, \$2, \$3, \$4}' | column -t"

# 3. Lists running processes in compact form.\n   Usage: procs [match] where [match] is an optional match string
# Usage: procs [<matchstr>]
#
procs="ssbx 'echo -e \"\033[1;92mOWNER\tPID\tCPU\tNAME\033[0m\"; if [[ \"{2}\" != \"<nil>\" ]]; then ps aux | grep \"{2}\" | grep -v \"grep\" | sed \"s/ \+/\	/g\" | cut -d \"	\" -f 4-10,13- --complement --output-delimiter=\"	\" | sed {1} | sed {1} | cut -c1-100 | grep --color=always \"{2}\"; else ps aux | grep -v \"grep\" | sed \"s/ \+/\	/g\" | cut -d \"	\" -f 4-10,13- --complement --output-delimiter=\"	\" | sed {1} | sed {1} | cut -c1-100 | more;fi' '\"s/\(	[^	]*\)\{4\}	/\1\1\1 /\"'"

# WRAPPER FUNCTIONS :

function _ssbx() {

    # Check the parameters passed in by the wrapper functions
    local runasroot="$1"
    shift
    local pageit="$1"
    shift
    local verboseout="$1"
    local isverbose=false
    if [ "$verboseout" -eq 1 ]; then
        isverbose=true
    fi
    shift

    # Extract the template and remaining arguments
    local mytemplate="$1"
    shift
    local args=("$@")
    local num_args="${#args[@]}"

    # Check if there are any remaining arguments
    if [ $num_args -ge 1 ]; then
        local checkarg="${args[-1]}"
    else
        local checkarg=""
    fi

    # Extract the comment from the template if it has one
    local iscomment=true
    local mycomment=""
    mycomment=$(echo "$mytemplate" | sed -n 's/^.*\$(: "\([^"]*\)" ).*$/\1/p')
    if [ -z "$mycomment" ]; then
        mycomment=$(echo "$mytemplate" | sed -n 's/^.*\$(:"\([^"]*\)").*$/\1/p')
        if [ -z "$mycomment" ]; then
            mycomment=""
            iscomment=false
        fi
    fi

    local BLUE='\e[94m'
    local ITALICS='\e[3m'
    local NC='\e[39m'

    # Check if the command is a help command
    if [[ "$checkarg" == "--help" || "$checkarg" == "-h" || "$checkarg" == "help" ]]; then
        if [ $iscomment = false ]; then
            mycomment="No help available for this command."
        fi
        echo
        echo -e "${BLUE}${ITALICS}${mycomment}"
        echo -e "$NC"
        tput ritm
    else
        
        # make {0} null (bug with safex which requires that {0} be thrown away)
        local new_args=("${mytemplate}" "" "${args[@]}")

        # create a temporary file under the user's account
        local temp_file=$(sudo bash -c 'USER=npepin; '"echo $(mktemp)")
        /usr/bin/safex "${new_args[@]}" >$temp_file

        # replace all instances of <nil> with "" in the temp file
        sed -i 's/<nil>/\"\"/g' $temp_file

        if [[ $isverbose == true ]]; then
            # if verbose output was requested and there is an embedded  comment, remove the comment from the command so it does
            # the othput is not cluttered
            if [[ $iscomment == true ]]; then
                sed -i 's/\$(:[^)]*) && //g' "$temp_file"
            fi
            echo -en "${BLUE}${ITALICS}executing: "
            cat "$temp_file"
            echo -e "$NC"
            tput ritm
        fi

        # execute the command in the temp file as root and have it paged;
        # (appending '| cat' in the template argument will supress paging)
        export temp_file
        if [ "$pageit" -eq 1 ]; then
            if [ "$runasroot" -eq 1 ]; then
                sudo /usr/bin/unbuffer bash -c 'USER=root; '"$(cat $temp_file)" | more
            else
                /usr/bin/unbuffer bash -c 'USER=npepin; '"$(cat $temp_file)" | more
            fi
        else
            if [ "$runasroot" -eq 1 ]; then
                eval "$(cat $temp_file)"
            else
                sudo bash -c 'USER=npepin; '"$(cat $temp_file)"
            fi
        fi
        # echo -e "$NC"
        tput ritm

        # Ensure the temporary file is deleted on script exit or if interrupted
        trap "rm -f $temp_file" EXIT
    fi
}
export -f _ssbx

# run as root with pagination
function ssbxp() {
    local args=("$@")
    local runasroot=1
    local pageit=1
    local verboseout=0
    args=("$runasroot" "$pageit" "$verboseout" "${args[@]}")
    _ssbx "${args[@]}"
}
export -f ssbxp

# run as root without pagination
function ssbx() {
    local args=("$@")
    local runasroot=1
    local pageit=0
    local verboseout=0
    args=("$runasroot" "$pageit" "$verboseout" "${args[@]}")
    _ssbx "${args[@]}"
}
export -f ssbx

# run as root without pagination and with verbose output
function vssbx() {
    local args=("$@")
    local runasroot=1
    local pageit=0
    local verboseout=1
    args=("$runasroot" "$pageit" "$verboseout" "${args[@]}")
    _ssbx "${args[@]}"
}
export -f vssbx

# run as user with pagination 
function ssbxp() {
    local args=("$@")
    local runasroot=0
    local pageit=1
    local verboseout=0
    args=("$runasroot" "$pageit" "$verboseout" "${args[@]}")
    _ssbx "${args[@]}"
}
export -f ssbxp

# run as user without pagination 
function sbx() {
    local args=("$@")
    local runasroot=0
    local pageit=0
    local verboseout=0
    args=("$runasroot" "$pageit" "$verboseout" "${args[@]}")
    _ssbx "${args[@]}"
}
export -f sbx
