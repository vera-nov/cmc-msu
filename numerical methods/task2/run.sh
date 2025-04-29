g++ main.cpp -o main

if [ $? -eq 0 ]; then
    ./main
else
    echo "Ошибка компиляции"
fi
