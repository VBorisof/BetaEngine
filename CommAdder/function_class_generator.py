from consts import *
from generator_input import *

class FunctionClassGenerator:
    def generate_interpreter_args(self, input):
        result = ''
        arg_count = 0
        if input.int_args != None:
            for _ in input.int_args:
                result += f', (int)((double)arguments[{arg_count}])'
                arg_count += 1
        if input.str_args != None:
            for _ in input.str_args:
                result += f', (string) arguments[{arg_count}]'
                arg_count += 1
        if input.bool_args != None:
            for _ in input.bool_args:
                result += f', (bool) arguments[{arg_count}]'
                arg_count += 1
        if input.custom_args != None:
            for arg in input.custom_args:
                result += f', ({arg[0]}) arguments[{arg_count}]'
                arg_count += 1

        return result

    def generate_error_args(self, input):
        result = ''
        if input.int_args != None:
            for _ in input.int_args:
                result += f'(int) '
        if input.str_args != None:
            for _ in input.str_args:
                result += f'(string) '
        if input.bool_args != None:
            for _ in input.bool_args:
                result += f'(bool) '
        if input.custom_args != None:
            for arg in input.custom_args:
                result += f'({arg[0]}) '

        return result

    def generate(self, input: GeneratorInput):
        command_name = input.command_name
        command_name = command_name[0].upper() + command_name[1:]
        interpreter_args = self.generate_interpreter_args(input)

        template_function = "template_method.txt" if input.isMethod else "template_function.txt"

        result = ''
        with open(template_function, 'r') as f:
            for line in f.readlines():
                line = line.replace('$name', command_name)
                line = line.replace('$arity', str(input.arity()))
                line = line.replace('$args', interpreter_args)
                line = line.replace('$funcname', command_name.lower())
                line = line.replace('$error_args', self.generate_error_args(input))
                result += line

        return result

