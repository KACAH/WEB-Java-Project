public class ArgsSorting {    
    public static void main(String[] args) throws Throwable {
        if (args.length > 0) {
            double[] sorted = new double [args.length];
            for(int i=0; i < args.length; i++) {
                sorted[i] = Double.parseDouble(args[i]);
            }
            java.util.Arrays.sort(sorted);
            for(int i=0; i < args.length; i++) {
                System.out.print(sorted[i]);
                System.out.print("; ");
            }
            System.out.println();
        } else {
            System.out.println("Invalid arguments count!");
        }
    }
}
