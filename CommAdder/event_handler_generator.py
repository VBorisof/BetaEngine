from consts import *
from generator_input import *

class EventHandlerGenerator:
    def generate_eventhandlers(self, functionNames: list[str]):
        result = ''
        for functionName in functionNames:
            result += f'{TAB}public EventHandler<{functionName}EventArgs> On{functionName} = (_, __) => {{}};\n'
        return result

    def generate(self, input: GeneratorInput):
        template_eventhandlers = 'template_eventhandlers.txt'

        result = ''
        with open(template_eventhandlers, 'r') as template_file:
            for line in template_file.readlines():
                line = line.replace('$eventhandlers', self.generate_eventhandlers(input.functions))
                result += line

        return result

