# SymbolFinder
A tool for finding the existence of a symbol inside of the Couchbase.Lite library.  Used to verify that CE only has what it should.

# Usage
<pre>
Usage: SymbolFinder [options]

Options:
  -?|-h|--help       Show help information
  -s|--symbol-file <SYMBOL_FILE>  The list of symbols to check for non-existence
  -t|--trace:<TRACE>        The amount of logging to perform while running
</pre>

The return value of the program is the number of symbols that were found (in other words, the normal return code of 0 is desired for CE)

# Symbol File Format
One symbol per line to search for, the format is as follows.  Note that symbol_name is the fully namespace qualified name of the symbol (e.g. not Database, but Couchbase.Lite.Database)

- c:<symbol_name> searches for a class with the given name (c: prefix is optional)<br />
- p:<symbol_name> searches for a property on a given class with the given name<br />
- m:<symbol_name>(type1, type2, ...) searchfor for a method on a given class with the given name and argument types (types do not need to be the fully qualified name)
