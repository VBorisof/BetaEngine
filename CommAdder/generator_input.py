class GeneratorInput:
    command_name = ""
    int_args: list[str] = []
    str_args: list[str] = []
    bool_args: list[str] = []
    custom_args: list[list[str]] = []
    isMethod: bool = False

    # This stores all defined API functions.
    # not specific to currently generated command.
    functions: list[str] = []

    def arity(self):
        return len(self.int_args) \
                + len(self.str_args) \
                + len(self.bool_args) \
                + len(self.custom_args)

