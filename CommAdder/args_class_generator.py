from consts import *
from generator_input import *

class ArgsClassGenerator:
    def generate_args_properties(self, input):
        result = ''
        if input.int_args != None:
            for arg in input.int_args:
                arg = arg[0].upper() + arg[1:]
                result += f'\n{TAB}public int ' + arg + ' { get; set; }' 
        if input.str_args != None:
            for arg in input.str_args:
                arg = arg[0].upper() + arg[1:]
                result += f'\n{TAB}public string ' + arg + ' { get; set; }' 
        if input.bool_args != None:
            for arg in input.bool_args:
                arg = arg[0].upper() + arg[1:]
                result += f'\n{TAB}public bool ' + arg + ' { get; set; }' 
        if input.custom_args != None:
            for arg in input.custom_args:
                type = arg[0]
                arg = arg[1][0].upper() + arg[1][1:]
                result += f'\n{TAB}public {type} {arg}' + ' { get; set; }' 

        return result

    def generate_args_params(self, input):
        result = ''
        if input.int_args != None:
            for arg in input.int_args:
                arg = arg[0].lower() + arg[1:]
                result += ', int ' + arg 
        if input.str_args != None:
            for arg in input.str_args:
                arg = arg[0].lower() + arg[1:]
                result += ', string ' + arg 
        if input.bool_args != None:
            for arg in input.bool_args:
                arg = arg[0].lower() + arg[1:]
                result += ', bool ' + arg 
        if input.custom_args != None:
            for arg in input.custom_args:
                type = arg[0]
                arg = arg[1][0].lower() + arg[1][1:]
                result += f', {type} ' + arg 

        return result

    def generate_ctor_init(self, input):
        result = ''
        if input.int_args != None:
            for arg in input.int_args:
                prop = arg[0].upper() + arg[1:]
                param = arg[0].lower() + arg[1:]
                result += f'\n{TAB*2}' + prop + " = " + param + ";"
        if input.str_args != None:
            for arg in input.str_args:
                prop = arg[0].upper() + arg[1:]
                param = arg[0].lower() + arg[1:]
                result += f'\n{TAB*2}' + prop + " = " + param + ";"
        if input.bool_args != None:
            for arg in input.bool_args:
                prop = arg[0].upper() + arg[1:]
                param = arg[0].lower() + arg[1:]
                result += f'\n{TAB*2}' + prop + " = " + param + ";"
        if input.custom_args != None:
            for arg in input.custom_args:
                prop = arg[1][0].upper() + arg[1][1:]
                param = arg[1][0].lower() + arg[1][1:]
                result += f'\n{TAB*2}' + prop + " = " + param + ";"

        return result

    def generate(self, input: GeneratorInput):
        command_name = input.command_name
        command_name = command_name[0].upper() + command_name[1:]
        command_props = self.generate_args_properties(input)
        command_params = self.generate_args_params(input)
        command_init = self.generate_ctor_init(input)

        template_args = "template_args.txt"
        
        result = ''
        with open(template_args, 'r') as f:
            for line in f.readlines():
                line = line.replace('$name', command_name)
                line = line.replace('$args', command_props)
                line = line.replace('$params', command_params)
                line = line.replace('$init', command_init)
                result += line

        return result

