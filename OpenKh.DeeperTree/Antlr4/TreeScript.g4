grammar TreeScript;

script: NL* (statement NL*)* EOF;
statement: property | array | block;

property: name = token value = token;
array: name = token NL* '[' NL* (value = token NL*)* NL* ']';
block: name = token NL* '{' NL* (statement NL*)* NL* '}';

token: Bare | Quoted;

Bare: ~[ \r\n"{}\u005b\u005d]+;
Quoted: '"' ~["]* '"';

WS: [ \t] -> skip;
NL: '\r\n' | '\n' | '\r';

LINE_COMMENT: ';' ~[\r\n]* -> skip;
