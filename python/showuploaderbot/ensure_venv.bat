@echo off

echo running %0...
:: echo updating with git...
:: git pull

if exist venv\ (
    echo venv already exists
) else (
    echo making python venv
    python -m venv venv
    venv\Scripts\activate
    echo installing pip modules
    pip install -r REQUIREMENTS.txt
)

echo %0 finished
