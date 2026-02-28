from consts import *
from generator_input import *

class ClientInterfaceGenerator:
    def generate_eventhandlers(self, functionNames: list[str]):
        result = ''
        for functionName in functionNames:
            result += f'{TAB}protected abstract void On{functionName}(object? sender, {functionName}EventArgs e);\n'
        return result

    def generate_register(self, functionNames: list[str]):
        result = ''
        for functionName in functionNames:
            result += f'{TAB*2}_bdsmDriver.Interpreter.EventHandlers.On{functionName} += On{functionName};\n'
        return result

    def generate(self, input: GeneratorInput):
        template_eventhandlers = 'template_clientinterface.txt'

        result = ''
        with open(template_eventhandlers, 'r') as template_file:
            for line in template_file.readlines():
                line = line.replace('$eventhandlers', self.generate_eventhandlers(input.functions))
                line = line.replace('$register', self.generate_register(input.functions))
                result += line

        return result

