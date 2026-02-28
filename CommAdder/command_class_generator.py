from consts import *
from generator_input import *

class CommandClassGenerator:
    def generate_ctor_args(self, input):
        result = ''
        if input.isMethod:
            result += 'Actor actor, '

        if input.int_args != None:
            for arg in input.int_args:
                arg = arg[0].lower() + arg[1:]
                result += f'int {arg}, '
        if input.str_args != None:
            for arg in input.str_args:
                arg = arg[0].lower() + arg[1:]
                result += f'string {arg}, '
        if input.bool_args != None:
            for arg in input.bool_args:
                arg = arg[0].lower() + arg[1:]
                result += f'bool {arg}, '
        if input.custom_args != None:
            for arg in input.custom_args:
                type = arg[0]
                arg = arg[1][0].lower() + arg[1][1:]
                result += f'{type} {arg}, '

        return result.strip(', ')

    def generate_props(self, input):
        result = ''
        if input.int_args != None:
            for arg in input.int_args:
                arg = arg[0].upper() + arg[1:]
                result += f'{TAB}public int {arg} {{ get; set; }}\n'
        if input.str_args != None:
            for arg in input.str_args:
                arg = arg[0].upper() + arg[1:]
                result += f'{TAB}public string {arg} {{ get; set; }}\n'
        if input.bool_args != None:
            for arg in input.bool_args:
                arg = arg[0].upper() + arg[1:]
                result += f'{TAB}public bool {arg} {{ get; set; }}\n'
        if input.custom_args != None:
            for arg in input.custom_args:
                type = arg[0]
                arg = arg[1][0].upper() + arg[1][1:]
                result += f'{TAB}public {type} {arg} {{ get; set; }}\n'

        return result.strip(', ')

    def generate_init(self, input):
        result = ''
        if input.int_args != None:
            for arg in input.int_args:
                prop = arg[0].upper() + arg[1:]
                arg = arg[0].lower() + arg[1:]
                result += f'{TAB*2}{prop} = {arg};\n'
        if input.str_args != None:
            for arg in input.str_args:
                prop = arg[0].upper() + arg[1:]
                arg = arg[0].lower() + arg[1:]
                result += f'{TAB*2}{prop} = {arg};\n'
        if input.bool_args != None:
            for arg in input.bool_args:
                prop = arg[0].upper() + arg[1:]
                arg = arg[0].lower() + arg[1:]
                result += f'{TAB*2}{prop} = {arg};\n'
        if input.custom_args != None:
            for arg in input.custom_args:
                prop = arg[1][0].upper() + arg[1][1:]
                arg = arg[1][0].lower() + arg[1][1:]
                result += f'{TAB*2}{prop} = {arg};\n'

        return result.strip(', ')

    def generate(self, input: GeneratorInput):
        command_name = input.command_name
        command_name = command_name[0].upper() + command_name[1:]
        ctor_args = self.generate_ctor_args(input)

        template_function = "template_command.txt"

        result = ''
        with open(template_function, 'r') as f:
            for line in f.readlines():
                line = line.replace('$name', command_name)
                line = line.replace('$ctorargs', ctor_args)
                line = line.replace('$actorset', ' : base(actor)' if input.isMethod else '')
                line = line.replace('$props', self.generate_props(input))
                line = line.replace('$init', self.generate_init(input))
                result += line

        return result
