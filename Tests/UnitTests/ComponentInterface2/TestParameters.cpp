//---------------------------------------------------------------------------

#include <ComponentInterface2/DataTypes.h>
#include <ComponentInterface2/ScienceAPI2Impl.h>
#include <ComponentInterface2/CMPComponentInterface.h>
#include <ComponentInterface2/MessageData.h>
#include <ComponentInterface2/Messages.h>
#include <cppunit/extensions/HelperMacros.h>
#include "TestParameters.h"

extern CMPComponentInterface* componentInterface;
extern ScienceAPI2* scienceAPI;
extern unsigned messageArg;
extern int parentID;
extern int componentID;
void STDCALL PMCallback(const unsigned* arg, Message& message);
void setup();
void teardown();

static const char* simScript = "<component name=\"ABC\" executable=\"abc.dll\">"
                               "   <initdata>"
                               "      <a>1.0</a>"
                               "      <b>2.0</b>"
                               "      <c>Three</c>"
                               "      <parameters2>"
                               "         <a>4.0</a>"
                               "         <b>5.0</b>"
                               "         <c>Six</c>"
                               "      </parameters2>"
                               "   </initdata>"
                               "</component>";

//---------------------------------------------------------------------------
// setup the component/PM testbed.
//---------------------------------------------------------------------------
void sendInit1()
   {
   Init1Type init1;
   init1.sdml = simScript;
   init1.fqn = "parent.abc";
   init1.inStartup = true;
   componentInterface->messageToLogic(newMessage(Message::Init1, parentID, componentID, false, init1));
   }

class ParametersTestCase : public CppUnit::TestFixture { 
public:

//---------------------------------------------------------------------------
//---------------------------------------------------------------------------
//---------------------------------------------------------------------------
// UseCase: read some numeric values from parameter set.
//---------------------------------------------------------------------------
void ReadParameter()
   {
   setup();
   sendInit1();

   CPPUNIT_ASSERT(scienceAPI->name() == "abc");
   CPPUNIT_ASSERT(scienceAPI->FQName() == "parent.abc");

   float a;
   scienceAPI->read("a", "", false, a, (float)0.0, (float)1.0);
   CPPUNIT_ASSERT(a == 1);

   scienceAPI->read("a", "", false, a, (float)0.0, (float)0.5);
   CPPUNIT_ASSERT(a == 1);

   int b;
   scienceAPI->read("b", "", false, b, 0, 4);
   CPPUNIT_ASSERT(b == 2);

   scienceAPI->read("b", "", false, b, 0, 1);
   CPPUNIT_ASSERT(b == 2);

   string c;
   scienceAPI->read("c", "", false, c);
   CPPUNIT_ASSERT(c == "Three");

   CPPUNIT_ASSERT(!scienceAPI->read("notfound", "", true, c));

   teardown();
   }
};

//---------------------------------------------------------------------------
// test method
//---------------------------------------------------------------------------
CppUnit::TestSuite * TestParameters() 
   {
   CppUnit::TestSuite *suite= new CppUnit::TestSuite("Parameters_test_suite" );
   suite->addTest(new CppUnit::TestCaller<ParametersTestCase>("ReadParameter", &ParametersTestCase::ReadParameter) );
   return(suite);
   }




