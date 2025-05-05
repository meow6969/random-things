@echo off

echo running %0...
git pull

if exist venv\ (
    echo venv already exists
) else (
    echo making python venv
    python -m venv venv
    venv\Scripts\activate
    pip install -r REQUIREMENTS.txt
)

echo %0 finished
