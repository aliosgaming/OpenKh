//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.11.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from TreeScript.g4 by ANTLR 4.11.1

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using DFA = Antlr4.Runtime.Dfa.DFA;

[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.11.1")]
[System.CLSCompliant(false)]
public partial class TreeScriptParser : Parser {
	protected static DFA[] decisionToDFA;
	protected static PredictionContextCache sharedContextCache = new PredictionContextCache();
	public const int
		T__0=1, T__1=2, T__2=3, T__3=4, Bare=5, Quoted=6, WS=7, NL=8, LINE_COMMENT=9;
	public const int
		RULE_script = 0, RULE_statement = 1, RULE_property = 2, RULE_array = 3, 
		RULE_block = 4, RULE_token = 5;
	public static readonly string[] ruleNames = {
		"script", "statement", "property", "array", "block", "token"
	};

	private static readonly string[] _LiteralNames = {
		null, "'['", "']'", "'{'", "'}'"
	};
	private static readonly string[] _SymbolicNames = {
		null, null, null, null, null, "Bare", "Quoted", "WS", "NL", "LINE_COMMENT"
	};
	public static readonly IVocabulary DefaultVocabulary = new Vocabulary(_LiteralNames, _SymbolicNames);

	[NotNull]
	public override IVocabulary Vocabulary
	{
		get
		{
			return DefaultVocabulary;
		}
	}

	public override string GrammarFileName { get { return "TreeScript.g4"; } }

	public override string[] RuleNames { get { return ruleNames; } }

	public override int[] SerializedAtn { get { return _serializedATN; } }

	static TreeScriptParser() {
		decisionToDFA = new DFA[_ATN.NumberOfDecisions];
		for (int i = 0; i < _ATN.NumberOfDecisions; i++) {
			decisionToDFA[i] = new DFA(_ATN.GetDecisionState(i), i);
		}
	}

		public TreeScriptParser(ITokenStream input) : this(input, Console.Out, Console.Error) { }

		public TreeScriptParser(ITokenStream input, TextWriter output, TextWriter errorOutput)
		: base(input, output, errorOutput)
	{
		Interpreter = new ParserATNSimulator(this, _ATN, decisionToDFA, sharedContextCache);
	}

	public partial class ScriptContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode Eof() { return GetToken(TreeScriptParser.Eof, 0); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode[] NL() { return GetTokens(TreeScriptParser.NL); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode NL(int i) {
			return GetToken(TreeScriptParser.NL, i);
		}
		[System.Diagnostics.DebuggerNonUserCode] public StatementContext[] statement() {
			return GetRuleContexts<StatementContext>();
		}
		[System.Diagnostics.DebuggerNonUserCode] public StatementContext statement(int i) {
			return GetRuleContext<StatementContext>(i);
		}
		public ScriptContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_script; } }
	}

	[RuleVersion(0)]
	public ScriptContext script() {
		ScriptContext _localctx = new ScriptContext(Context, State);
		EnterRule(_localctx, 0, RULE_script);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 15;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while (_la==NL) {
				{
				{
				State = 12;
				Match(NL);
				}
				}
				State = 17;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			State = 27;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while (_la==Bare || _la==Quoted) {
				{
				{
				State = 18;
				statement();
				State = 22;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
				while (_la==NL) {
					{
					{
					State = 19;
					Match(NL);
					}
					}
					State = 24;
					ErrorHandler.Sync(this);
					_la = TokenStream.LA(1);
				}
				}
				}
				State = 29;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			State = 30;
			Match(Eof);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class StatementContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public PropertyContext property() {
			return GetRuleContext<PropertyContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public ArrayContext array() {
			return GetRuleContext<ArrayContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public BlockContext block() {
			return GetRuleContext<BlockContext>(0);
		}
		public StatementContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_statement; } }
	}

	[RuleVersion(0)]
	public StatementContext statement() {
		StatementContext _localctx = new StatementContext(Context, State);
		EnterRule(_localctx, 2, RULE_statement);
		try {
			State = 35;
			ErrorHandler.Sync(this);
			switch ( Interpreter.AdaptivePredict(TokenStream,3,Context) ) {
			case 1:
				EnterOuterAlt(_localctx, 1);
				{
				State = 32;
				property();
				}
				break;
			case 2:
				EnterOuterAlt(_localctx, 2);
				{
				State = 33;
				array();
				}
				break;
			case 3:
				EnterOuterAlt(_localctx, 3);
				{
				State = 34;
				block();
				}
				break;
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class PropertyContext : ParserRuleContext {
		public TokenContext name;
		public TokenContext value;
		[System.Diagnostics.DebuggerNonUserCode] public TokenContext[] token() {
			return GetRuleContexts<TokenContext>();
		}
		[System.Diagnostics.DebuggerNonUserCode] public TokenContext token(int i) {
			return GetRuleContext<TokenContext>(i);
		}
		public PropertyContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_property; } }
	}

	[RuleVersion(0)]
	public PropertyContext property() {
		PropertyContext _localctx = new PropertyContext(Context, State);
		EnterRule(_localctx, 4, RULE_property);
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 37;
			_localctx.name = token();
			State = 38;
			_localctx.value = token();
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class ArrayContext : ParserRuleContext {
		public TokenContext name;
		public TokenContext value;
		[System.Diagnostics.DebuggerNonUserCode] public TokenContext[] token() {
			return GetRuleContexts<TokenContext>();
		}
		[System.Diagnostics.DebuggerNonUserCode] public TokenContext token(int i) {
			return GetRuleContext<TokenContext>(i);
		}
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode[] NL() { return GetTokens(TreeScriptParser.NL); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode NL(int i) {
			return GetToken(TreeScriptParser.NL, i);
		}
		public ArrayContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_array; } }
	}

	[RuleVersion(0)]
	public ArrayContext array() {
		ArrayContext _localctx = new ArrayContext(Context, State);
		EnterRule(_localctx, 6, RULE_array);
		int _la;
		try {
			int _alt;
			EnterOuterAlt(_localctx, 1);
			{
			State = 40;
			_localctx.name = token();
			State = 44;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while (_la==NL) {
				{
				{
				State = 41;
				Match(NL);
				}
				}
				State = 46;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			State = 47;
			Match(T__0);
			State = 51;
			ErrorHandler.Sync(this);
			_alt = Interpreter.AdaptivePredict(TokenStream,5,Context);
			while ( _alt!=2 && _alt!=global::Antlr4.Runtime.Atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					State = 48;
					Match(NL);
					}
					} 
				}
				State = 53;
				ErrorHandler.Sync(this);
				_alt = Interpreter.AdaptivePredict(TokenStream,5,Context);
			}
			State = 63;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while (_la==Bare || _la==Quoted) {
				{
				{
				State = 54;
				_localctx.value = token();
				State = 58;
				ErrorHandler.Sync(this);
				_alt = Interpreter.AdaptivePredict(TokenStream,6,Context);
				while ( _alt!=2 && _alt!=global::Antlr4.Runtime.Atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						State = 55;
						Match(NL);
						}
						} 
					}
					State = 60;
					ErrorHandler.Sync(this);
					_alt = Interpreter.AdaptivePredict(TokenStream,6,Context);
				}
				}
				}
				State = 65;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			State = 69;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while (_la==NL) {
				{
				{
				State = 66;
				Match(NL);
				}
				}
				State = 71;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			State = 72;
			Match(T__1);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class BlockContext : ParserRuleContext {
		public TokenContext name;
		[System.Diagnostics.DebuggerNonUserCode] public TokenContext token() {
			return GetRuleContext<TokenContext>(0);
		}
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode[] NL() { return GetTokens(TreeScriptParser.NL); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode NL(int i) {
			return GetToken(TreeScriptParser.NL, i);
		}
		[System.Diagnostics.DebuggerNonUserCode] public StatementContext[] statement() {
			return GetRuleContexts<StatementContext>();
		}
		[System.Diagnostics.DebuggerNonUserCode] public StatementContext statement(int i) {
			return GetRuleContext<StatementContext>(i);
		}
		public BlockContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_block; } }
	}

	[RuleVersion(0)]
	public BlockContext block() {
		BlockContext _localctx = new BlockContext(Context, State);
		EnterRule(_localctx, 8, RULE_block);
		int _la;
		try {
			int _alt;
			EnterOuterAlt(_localctx, 1);
			{
			State = 74;
			_localctx.name = token();
			State = 78;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while (_la==NL) {
				{
				{
				State = 75;
				Match(NL);
				}
				}
				State = 80;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			State = 81;
			Match(T__2);
			State = 85;
			ErrorHandler.Sync(this);
			_alt = Interpreter.AdaptivePredict(TokenStream,10,Context);
			while ( _alt!=2 && _alt!=global::Antlr4.Runtime.Atn.ATN.INVALID_ALT_NUMBER ) {
				if ( _alt==1 ) {
					{
					{
					State = 82;
					Match(NL);
					}
					} 
				}
				State = 87;
				ErrorHandler.Sync(this);
				_alt = Interpreter.AdaptivePredict(TokenStream,10,Context);
			}
			State = 97;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while (_la==Bare || _la==Quoted) {
				{
				{
				State = 88;
				statement();
				State = 92;
				ErrorHandler.Sync(this);
				_alt = Interpreter.AdaptivePredict(TokenStream,11,Context);
				while ( _alt!=2 && _alt!=global::Antlr4.Runtime.Atn.ATN.INVALID_ALT_NUMBER ) {
					if ( _alt==1 ) {
						{
						{
						State = 89;
						Match(NL);
						}
						} 
					}
					State = 94;
					ErrorHandler.Sync(this);
					_alt = Interpreter.AdaptivePredict(TokenStream,11,Context);
				}
				}
				}
				State = 99;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			State = 103;
			ErrorHandler.Sync(this);
			_la = TokenStream.LA(1);
			while (_la==NL) {
				{
				{
				State = 100;
				Match(NL);
				}
				}
				State = 105;
				ErrorHandler.Sync(this);
				_la = TokenStream.LA(1);
			}
			State = 106;
			Match(T__3);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	public partial class TokenContext : ParserRuleContext {
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode Bare() { return GetToken(TreeScriptParser.Bare, 0); }
		[System.Diagnostics.DebuggerNonUserCode] public ITerminalNode Quoted() { return GetToken(TreeScriptParser.Quoted, 0); }
		public TokenContext(ParserRuleContext parent, int invokingState)
			: base(parent, invokingState)
		{
		}
		public override int RuleIndex { get { return RULE_token; } }
	}

	[RuleVersion(0)]
	public TokenContext token() {
		TokenContext _localctx = new TokenContext(Context, State);
		EnterRule(_localctx, 10, RULE_token);
		int _la;
		try {
			EnterOuterAlt(_localctx, 1);
			{
			State = 108;
			_la = TokenStream.LA(1);
			if ( !(_la==Bare || _la==Quoted) ) {
			ErrorHandler.RecoverInline(this);
			}
			else {
				ErrorHandler.ReportMatch(this);
			    Consume();
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			ErrorHandler.ReportError(this, re);
			ErrorHandler.Recover(this, re);
		}
		finally {
			ExitRule();
		}
		return _localctx;
	}

	private static int[] _serializedATN = {
		4,1,9,111,2,0,7,0,2,1,7,1,2,2,7,2,2,3,7,3,2,4,7,4,2,5,7,5,1,0,5,0,14,8,
		0,10,0,12,0,17,9,0,1,0,1,0,5,0,21,8,0,10,0,12,0,24,9,0,5,0,26,8,0,10,0,
		12,0,29,9,0,1,0,1,0,1,1,1,1,1,1,3,1,36,8,1,1,2,1,2,1,2,1,3,1,3,5,3,43,
		8,3,10,3,12,3,46,9,3,1,3,1,3,5,3,50,8,3,10,3,12,3,53,9,3,1,3,1,3,5,3,57,
		8,3,10,3,12,3,60,9,3,5,3,62,8,3,10,3,12,3,65,9,3,1,3,5,3,68,8,3,10,3,12,
		3,71,9,3,1,3,1,3,1,4,1,4,5,4,77,8,4,10,4,12,4,80,9,4,1,4,1,4,5,4,84,8,
		4,10,4,12,4,87,9,4,1,4,1,4,5,4,91,8,4,10,4,12,4,94,9,4,5,4,96,8,4,10,4,
		12,4,99,9,4,1,4,5,4,102,8,4,10,4,12,4,105,9,4,1,4,1,4,1,5,1,5,1,5,0,0,
		6,0,2,4,6,8,10,0,1,1,0,5,6,119,0,15,1,0,0,0,2,35,1,0,0,0,4,37,1,0,0,0,
		6,40,1,0,0,0,8,74,1,0,0,0,10,108,1,0,0,0,12,14,5,8,0,0,13,12,1,0,0,0,14,
		17,1,0,0,0,15,13,1,0,0,0,15,16,1,0,0,0,16,27,1,0,0,0,17,15,1,0,0,0,18,
		22,3,2,1,0,19,21,5,8,0,0,20,19,1,0,0,0,21,24,1,0,0,0,22,20,1,0,0,0,22,
		23,1,0,0,0,23,26,1,0,0,0,24,22,1,0,0,0,25,18,1,0,0,0,26,29,1,0,0,0,27,
		25,1,0,0,0,27,28,1,0,0,0,28,30,1,0,0,0,29,27,1,0,0,0,30,31,5,0,0,1,31,
		1,1,0,0,0,32,36,3,4,2,0,33,36,3,6,3,0,34,36,3,8,4,0,35,32,1,0,0,0,35,33,
		1,0,0,0,35,34,1,0,0,0,36,3,1,0,0,0,37,38,3,10,5,0,38,39,3,10,5,0,39,5,
		1,0,0,0,40,44,3,10,5,0,41,43,5,8,0,0,42,41,1,0,0,0,43,46,1,0,0,0,44,42,
		1,0,0,0,44,45,1,0,0,0,45,47,1,0,0,0,46,44,1,0,0,0,47,51,5,1,0,0,48,50,
		5,8,0,0,49,48,1,0,0,0,50,53,1,0,0,0,51,49,1,0,0,0,51,52,1,0,0,0,52,63,
		1,0,0,0,53,51,1,0,0,0,54,58,3,10,5,0,55,57,5,8,0,0,56,55,1,0,0,0,57,60,
		1,0,0,0,58,56,1,0,0,0,58,59,1,0,0,0,59,62,1,0,0,0,60,58,1,0,0,0,61,54,
		1,0,0,0,62,65,1,0,0,0,63,61,1,0,0,0,63,64,1,0,0,0,64,69,1,0,0,0,65,63,
		1,0,0,0,66,68,5,8,0,0,67,66,1,0,0,0,68,71,1,0,0,0,69,67,1,0,0,0,69,70,
		1,0,0,0,70,72,1,0,0,0,71,69,1,0,0,0,72,73,5,2,0,0,73,7,1,0,0,0,74,78,3,
		10,5,0,75,77,5,8,0,0,76,75,1,0,0,0,77,80,1,0,0,0,78,76,1,0,0,0,78,79,1,
		0,0,0,79,81,1,0,0,0,80,78,1,0,0,0,81,85,5,3,0,0,82,84,5,8,0,0,83,82,1,
		0,0,0,84,87,1,0,0,0,85,83,1,0,0,0,85,86,1,0,0,0,86,97,1,0,0,0,87,85,1,
		0,0,0,88,92,3,2,1,0,89,91,5,8,0,0,90,89,1,0,0,0,91,94,1,0,0,0,92,90,1,
		0,0,0,92,93,1,0,0,0,93,96,1,0,0,0,94,92,1,0,0,0,95,88,1,0,0,0,96,99,1,
		0,0,0,97,95,1,0,0,0,97,98,1,0,0,0,98,103,1,0,0,0,99,97,1,0,0,0,100,102,
		5,8,0,0,101,100,1,0,0,0,102,105,1,0,0,0,103,101,1,0,0,0,103,104,1,0,0,
		0,104,106,1,0,0,0,105,103,1,0,0,0,106,107,5,4,0,0,107,9,1,0,0,0,108,109,
		7,0,0,0,109,11,1,0,0,0,14,15,22,27,35,44,51,58,63,69,78,85,92,97,103
	};

	public static readonly ATN _ATN =
		new ATNDeserializer().Deserialize(_serializedATN);


}
