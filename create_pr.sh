#!/bin/bash

# Create Pull Request for Launch Preparations and Polish
# This script helps create a PR with the comprehensive changes

echo "🚀 Creating Pull Request for Launch Preparations and Polish"
echo "=========================================================="

# Check if we're on the right branch
CURRENT_BRANCH=$(git branch --show-current)
echo "Current branch: $CURRENT_BRANCH"

if [ "$CURRENT_BRANCH" != "cursor/finalize-launch-preparations-and-polish-b4ee" ]; then
    echo "❌ Not on the correct branch. Please switch to cursor/finalize-launch-preparations-and-polish-b4ee"
    exit 1
fi

# Check if branch is up to date
echo "📡 Checking if branch is up to date..."
git fetch origin
LOCAL=$(git rev-parse @)
REMOTE=$(git rev-parse @{u})

if [ "$LOCAL" = "$REMOTE" ]; then
    echo "✅ Branch is up to date"
else
    echo "❌ Branch is not up to date. Please push changes first."
    exit 1
fi

echo ""
echo "📋 Pull Request Summary:"
echo "========================"
echo "• Branch: $CURRENT_BRANCH"
echo "• Target: main"
echo "• Files changed: $(git diff --name-only main | wc -l)"
echo "• Commits ahead: $(git rev-list --count main..HEAD)"
echo ""

echo "📝 PR Description saved to: PR_DESCRIPTION.md"
echo ""
echo "🔗 To create the PR manually:"
echo "1. Go to: https://github.com/[REPO]/compare/main...$CURRENT_BRANCH"
echo "2. Copy the content from PR_DESCRIPTION.md"
echo "3. Paste it as the PR description"
echo "4. Set title: '🚀 Finalize Launch Preparations and Polish - Complete Build Validation & Launch Readiness'"
echo "5. Create the pull request"
echo ""

echo "📊 Changes Summary:"
echo "==================="
git log --oneline main..HEAD

echo ""
echo "✅ Ready to create Pull Request!"
echo "Riley: 'Time to launch this cyberpunk dog-chase game to the world!'"
echo "Nibble: 'Bark! (Translation: Let's go build some games!)' 🐕💫"