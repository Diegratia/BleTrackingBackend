#!/bin/bash

input=$(cat)
model=$(echo "$input" | jq -r '.model.display_name')
dir=$(echo "$input" | jq -r '.workspace.current_dir')

# Get git branch if in a git repo
branch=""
if git rev-parse --git-dir > /dev/null 2>&1; then
    current_branch=$(git branch --show-current 2>/dev/null)
    if [ -n "$current_branch" ]; then
        # Check if there are uncommitted changes
        status=$(git status --porcelain 2>/dev/null)
        if [ -n "$status" ]; then
            branch="🌿 $current_branch*"
        else
            branch="🌿 $current_branch"
        fi
    fi
fi

# Calculate token usage
tokens_used=$(echo "$input" | jq -r '.model.usage // 0')
tokens_max=$(echo "$input" | jq -r '.model.max_tokens // 200000')

# Calculate percentage
if [ "$tokens_max" -gt 0 ]; then
    percent=$((tokens_used * 100 / tokens_max))
else
    percent=0
fi

# Build statusline parts
parts=()
if [ -n "$branch" ]; then
    parts+=("$branch")
fi
parts+=("📁 $dir")
parts+=("🤖 $model")
parts+=("📊 ${percent}% tokens")

# Join with " | "
statusline=$(IFS=" | "; echo "${parts[*]}")

printf "%s" "$statusline"
