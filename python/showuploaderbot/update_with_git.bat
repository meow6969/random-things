@echo off

echo restoring changes made...
git restore .
echo updating with git
git pull

echo now updated with git branch

set /P meow="press enter to exit..."
