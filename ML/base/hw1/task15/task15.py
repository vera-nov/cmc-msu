from typing import List


def hello(name: str=None) -> str:
    if name == None or len(name) == 0:
        return "Hello!"
    else:
        return "Hello, " + name + "!"


def int_to_roman(num: int) -> str:
    
    def int_to_roman_help(n=100, d1='C', d2='D', d3='M'):
        digit = (num // n) % 10
        if digit > 0 and digit < 4:
            return d1 * digit
        elif digit == 4:
            return d1 + d2
        elif digit == 5:
            return d2
        elif digit > 5 and digit < 9:
            return d2 + d1 * (digit - 5)
        elif digit == 9:
            return d1 + d3
        return ''
        
    return 'M' * (num // 1000) + int_to_roman_help() + int_to_roman_help(10, 'X', 'L', 'C') + int_to_roman_help(1, 'I', 'V', 'X')


def longest_common_prefix(strs_input: List[str]=None) -> str:
    if strs_input == []:
        return ''
    res = ''
    strs_input = [s.strip() for s in strs_input]
    shortest = min(strs_input, key=len)
    for i in range(len(shortest)):
        for string in strs_input:
            if string[i] != shortest[i]:
                return res
        res += shortest[i]
    return res


class BankCard:
    
    def __init__(self, total_sum: int, balance_limit: int=-1): # balance_limit = -1 if unlimited
        self.total_sum = total_sum
        self.balance_limit = balance_limit
        # self.balance_check_counter = 0
        
    def __call__(self, sum_spent):
        if sum_spent > self.total_sum:
            print(f"Not enough money to spend {sum_spent} dollars.")
            raise ValueError
        self.total_sum -= sum_spent
        print(f"You spent {sum_spent} dollars.")

    def __repr__(self):
        return "To learn the balance call balance."

    def __str__(self):
        return "To learn the balance call balance."

    @property
    def balance(self):
        if self.balance_limit == 0:
            print("Balance check limits exceeded.")
            raise ValueError
        elif self.balance_limit > 0:
            self.balance_limit -= 1
        return self.total_sum

    def put(self, sum_put):
        self.total_sum += sum_put
        print(f"You put {sum_put} dollars.")

    def __add__(self, other):
        new_total_sum = self.total_sum + other.total_sum
        new_balance_limit = -1 if (self.balance_limit < 0 or other.balance_limit < 0) else max(self.balance_limit, other.balance_limit)
        return BankCard(new_total_sum, new_balance_limit)
        

def primes() -> int:
    
    def is_prime(n):
        for i in range(2, int(n ** 0.5) + 1):
            if n % i == 0:
                return False
        return True
        
    num = 2
    while True:
        if is_prime(num):
            yield num
        num += 1

