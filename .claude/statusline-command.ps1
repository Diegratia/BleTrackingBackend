# Convert JSON input from stdin
$input = [Console]::In.ReadToEnd() | ConvertFrom-Json

# Extract information
$model = $input.model.display_name
$dir = $input.workspace.current_dir

# Get git branch if in a git repo
$branch = ""
try {
    $gitDir = git rev-parse --git-dir 2>$null
    if ($gitDir) {
        $currentBranch = git branch --show-current 2>$null
        if ($currentBranch) {
            # Check if there are uncommitted changes
            $status = git status --porcelain 2>$null
            if ($status) {
                $branch = "🌿 $currentBranch*"
            } else {
                $branch = "🌿 $currentBranch"
            }
        }
    }
} catch {
    # Not in a git repo
}

# Calculate token usage
$tokensUsed = $input.model.usage
if (-not $tokensUsed) { $tokensUsed = 0 }

$maxTokens = $input.model.max_tokens
if (-not $maxTokens) { $maxTokens = 200000 }

$percent = [math]::Round(($tokensUsed / $maxTokens) * 100)

# Build statusline parts
$parts = @()
if ($branch) {
    $parts += $branch
}
$parts += "📁 $dir"
$parts += "🤖 $model"
$parts += "📊 ${percent}% tokens"

# Join with " | "
$statusline = $parts -join " | "

# Output
Write-Output $statusline
